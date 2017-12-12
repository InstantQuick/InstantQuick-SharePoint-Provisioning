using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint.Client.Utilities;
using static IQAppProvisioningBaseClasses.Utility.Tokenizer;

namespace IQAppManifestBuilders
{
    public class ListCreatorBuilder : CreatorBuilderBase
    {
        public string GetListCreator(ClientContext ctx, Web web, string listName)
        {
            var manifest = new AppManifestBase();
            GetListCreator(ctx, web, listName, manifest);
            if (manifest.ListCreators.ContainsKey(listName))
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.ListCreators[listName]);
            }
            OnVerboseNotify("NO INFORMATION FOUND FOR " + listName);
            return string.Empty;
        }

        public void GetListCreator(ClientContext ctx, Web web, string listName, AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingListCreators = manifest.ListCreators ?? new Dictionary<string, ListCreator>();
            existingListCreators = existingListCreators ?? new Dictionary<string, ListCreator>();

            var listCreator = GetListCreatorFromSite(ctx, web, listName, manifest);

            if (listCreator != null)
            {
                existingListCreators[listName] = listCreator;
            }
            manifest.ListCreators = existingListCreators;
        }

        public void GetListCreatorListItems(ClientContext ctx, Web web, string listName, AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingListCreators = manifest.ListCreators;
            existingListCreators = existingListCreators ?? new Dictionary<string, ListCreator>();

            if (!existingListCreators.ContainsKey(listName))
                throw new ArgumentException("The list provided is not in the manifest.");

            var listCreator = existingListCreators[listName];
            listCreator.ListItems = GetListItems(ctx, web, listName);
            listCreator.ProvisionListItems = true;
            manifest.ListCreators = existingListCreators;
        }

        private ListCreator GetListCreatorFromSite(ClientContext ctx, Web web, string listName,
            AppManifestBase appManifest)
        {
            try
            {
                OnVerboseNotify("START reading list from web " + listName);

                var list = web.Lists.GetByTitle(listName);
                ctx.Load(web, w => w.ServerRelativeUrl);
                ctx.Load(web.AvailableFields,
                    fields =>
                        fields.Include(field => field.Title, field => field.Id, field => field.InternalName,
                            field => field.FieldTypeKind));
                ctx.Load(list,
                    l => l.Id,
                    l => l.Title,
                    l => l.Description,
                    l => l.RootFolder,
                    l => l.OnQuickLaunch,
                    l => l.TemplateFeatureId,
                    l => l.BaseTemplate,
                    l => l.DefaultView,
                    l => l.NoCrawl,
                    l => l.Hidden,
                    l => l.HasUniqueRoleAssignments,
                    l => l.RoleAssignments,
                    l => l.DefaultView,
                    l => l.DocumentTemplateUrl);

                ctx.Load(list.Fields, f => f.Where(field => field.Hidden == false));
                ctx.Load(list.ContentTypes,
                    ctypes => ctypes.Include(ct => ct.Id, ct => ct.Name, ct => ct.Parent, ct => ct.Fields, ct => ct.DocumentTemplateUrl));
                ctx.Load(list.Views,
                    v => v.Where(view => view.Hidden == false).Include(view => view.ListViewXml, view => view.Title));
                ctx.Load(list.WorkflowAssociations, wfa => wfa.Include(
                    wf => wf.AllowManual,
                    wf => wf.AssociationData,
                    wf => wf.AutoStartChange,
                    wf => wf.AutoStartCreate,
                    wf => wf.BaseId,
                    wf => wf.Description,
                    wf => wf.HistoryListTitle,
                    wf => wf.Id,
                    wf => wf.InstantiationUrl,
                    wf => wf.InternalName,
                    wf => wf.IsDeclarative,
                    wf => wf.Name,
                    wf => wf.TaskListTitle));

                ctx.ExecuteQueryRetry();

                OnVerboseNotify("END reading list from web " + listName);
                OnVerboseNotify("Building list creator for " + listName);

                var listCreator = new ListCreator
                {
                    Title = list.Title,
                    Id = list.Id.ToString(),
                    Description = list.Description,
                    Url =
                        web.ServerRelativeUrl == "/"
                            ? list.RootFolder.ServerRelativeUrl
                            : list.RootFolder.ServerRelativeUrl.Replace(web.ServerRelativeUrl, ""),
                    QuickLaunchOption = list.OnQuickLaunch ? QuickLaunchOptions.On : QuickLaunchOptions.Off,
                    TemplateFeatureId = list.TemplateFeatureId,
                    TemplateType = list.BaseTemplate,
                    NoCrawl = list.NoCrawl,
                    Hidden = list.Hidden,
                    ContentType = list.ContentTypes[0].Name,
                    AdditionalFields = new Dictionary<string, string>(),
                    IndexFields = new List<string>(),
                    HiddenFormFields = new List<string>(),
                    EnforceUniqueFields = new List<string>(),
                    FieldDisplayNameOverrides = new Dictionary<string, string>(),
                    AddToAllViewsFields = new List<string>(),
                    DisplayFormOnlyFields = new List<string>(),
                    ListViewSchemas = new Dictionary<string, string>()
                };

                if (list.DocumentTemplateUrl != null &&
                    !list.DocumentTemplateUrl.ToLowerInvariant().Contains("/template.") &&
                    ctx.Web.ServerRelativeUrl != "/")
                {
                    listCreator.DocumentTemplateUrl = TokenizeUrls(web, list.DocumentTemplateUrl);
                }
                else if(list.ContentTypes[0].DocumentTemplateUrl != null &&
                    !list.ContentTypes[0].DocumentTemplateUrl.ToLowerInvariant().Contains("/template.") &&
                    ctx.Web.ServerRelativeUrl != "/")
                {
                    listCreator.DocumentTemplateUrl = TokenizeUrls(web, list.ContentTypes[0].DocumentTemplateUrl);
                }

                //Some odd lists such as "Workflows" have no views
                try
                {
                    listCreator.DefaultViewTitle = list.DefaultView.Title;
                    listCreator.DefaultViewSchemaXml = CleanViewSchema(list.DefaultView.ListViewXml);
                }
                // ReSharper disable once UnusedVariable
                catch (ServerObjectNullReferenceException ex)
                {
                    //ignore
                }

                OnVerboseNotify("Getting related content types for " + listName);
                foreach (var ctype in list.ContentTypes)
                {
                    ctx.Load(ctype.Parent);
                    ctx.Load(ctype.Parent.Fields);
                    ctx.ExecuteQueryRetry();
                }
                foreach (var field in list.ContentTypes[0].Fields)
                {
                    AnalyzeListField(field, listCreator, list.ContentTypes, ctx);
                }
                AnalyzeContentTypes(listCreator, list);

                OnVerboseNotify("Analyzing views for " + listName);
                foreach (var view in list.Views)
                {
                    //Some older DataFormView customizations cause this to be null!
                    if (view.ListViewXml != null)
                    {
                        var schema = CleanViewSchema(view.ListViewXml);
                        if (schema != listCreator.DefaultViewSchemaXml)
                        {
                            listCreator.ListViewSchemas[view.Title] = schema;
                        }
                    }
                }

                OnVerboseNotify("Ananlyzing security for " + listName);
                AnalyzeSecurityConfiguration(listCreator, list, ctx);

                OnVerboseNotify("Checking for root folder properties for " + listName);
                CheckForPropertyBagItems(listCreator, list, ctx);

                OnVerboseNotify("Checking for lookup field for " + listName);
                CheckForCorrespondingLookupField(listCreator, list, ctx);

                OnVerboseNotify("Checking for user custom actions for " + listName);
                CheckForUserCustomActions(listCreator, list, ctx, appManifest);

                OnVerboseNotify("Checking for remote events for " + listName);
                CheckForRemoteEvents(listCreator, list, ctx, appManifest);

                OnVerboseNotify("Checking for workflow associations for " + listName);
                //TODO: For 2013 style workflows
                //CheckForWorkflowAssociations(listCreator, list, ctx, appManifest);

                return listCreator;
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
            return null;
        }

        //TODO: 2013 Style workflows
        //private void CheckForWorkflowAssociations(ListCreator listCreator, List list, ClientContext ctx, AppManifestBase appManifest)
        //{
        //    if (list.WorkflowAssociations.Count == 0) return;
        //    listCreator.WorkflowAssociations = new Dictionary<string, WorkflowAssociationCreator>();
        //    foreach (var wfa in list.WorkflowAssociations)
        //    {
        //        listCreator.WorkflowAssociations[wfa.Name] = new WorkflowAssociationCreator()
        //        {
        //            AllowManual = wfa.AllowManual,
        //            AssociationData = wfa.AssociationData,
        //            AutoStartChange = wfa.AutoStartChange,
        //            AutoStartCreate = wfa.AutoStartCreate,
        //            BaseId = wfa.BaseId,
        //            Description = wfa.Description,
        //            HistoryListTitle = wfa.HistoryListTitle,
        //            Id = wfa.Id,
        //            InstantiationUrl = wfa.InstantiationUrl,
        //            InternalName = wfa.InternalName,
        //            IsDeclarative = wfa.IsDeclarative,
        //            Name = wfa.Name,
        //            TaskListTitle = wfa.TaskListTitle
        //        };
        //    }
        //}

        private void CheckForPropertyBagItems(ListCreator listCreator, List list, ClientContext ctx)
        {
            var exclusions = new List<string>
            {
                "DesignPreview",
                "OriginalNotebookUrl",
                "vti_",
                "dlc_"
            };

            var propertyBagItems = new Dictionary<string, string>();
            var parentWeb = list.ParentWeb;
            ctx.Load(parentWeb, w => w.Url, w => w.ServerRelativeUrl);
            var rootFolderProps = list.RootFolder.Properties;
            ctx.Load(rootFolderProps);
            ctx.ExecuteQueryRetry();
            foreach (var p in rootFolderProps.FieldValues)
            {
                var exclude =
                    exclusions.FirstOrDefault(
                        preamble => p.Key.StartsWith(preamble, StringComparison.OrdinalIgnoreCase)) != null;

                if (!exclude)
                {
                    propertyBagItems[p.Key] = TokenizeUrls(list.ParentWeb, p.Value.ToString());
                }
            }
            if (propertyBagItems.Count > 0)
            {
                listCreator.PropertyBagItems = propertyBagItems;
            }
        }

        private void CheckForRemoteEvents(ListCreator listCreator, List list, ClientContext ctx,
            AppManifestBase appManifest)
        {
            var builder = new RemoteEventRegistrationCreatorBuilder();
            appManifest.ListCreators = new Dictionary<string, ListCreator> { [listCreator.Title] = new ListCreator() };

            builder.GetRemoteEventRegistrationCreators(ctx, list, listCreator.Title, appManifest);
            listCreator.RemoteEventRegistrationCreators =
                appManifest.ListCreators[listCreator.Title].RemoteEventRegistrationCreators;
        }

        private void CheckForUserCustomActions(ListCreator listCreator, List list, ClientContext ctx,
            AppManifestBase appManifest)
        {
            var builder = new CustomActionCreatorBuilder();
            appManifest.ListCreators = new Dictionary<string, ListCreator> { [listCreator.Title] = new ListCreator() };

            builder.GetCustomActionCreators(ctx, list, listCreator.Title, appManifest);
            listCreator.CustomActionCreators = appManifest.ListCreators[listCreator.Title].CustomActionCreators;
        }

        private void CheckForCorrespondingLookupField(ListCreator creator, List list, ClientContext ctx)
        {
            var lookupFields = new List<FieldLookup>();
            var fields = ctx.Web.AvailableFields;
            foreach (var field in fields)
            {
                if (field.FieldTypeKind == FieldType.Lookup)
                {
                    var lookupField = (FieldLookup)field;
                    ctx.Load(lookupField);
                    lookupFields.Add(lookupField);
                }
            }
            ctx.ExecuteQueryRetry();
            foreach (var lookup in lookupFields)
            {
                if (!string.IsNullOrEmpty(lookup.LookupList))
                {
                    //This is a rule of thumb hack for older CAML that has the URL instead of an ID
                    //Sometimes, the URL will have a Resource$ element. If it's the last part there is no good workaround as there is no way to read the resource file,
                    //But in the fab40 at least, it is the root list folder that is tokenized
                    //This will be wrong sometimes, but it is better than nothing
                    var listUrlParts = lookup.LookupList.Split('/');
                    var lastUrlPart = "/" + listUrlParts[listUrlParts.Length - 1];

                    if (lookup.LookupList == list.Id.ToString() ||
                        lookup.LookupList.ToLowerInvariant().Contains(list.Id.ToString().ToLowerInvariant()) ||
                        creator.Url.ToLower().Contains(lastUrlPart.ToLower()))
                    {
                        if (creator.CorrespondingLookupFieldNames == null)
                            creator.CorrespondingLookupFieldNames = new List<string>();
                        creator.CorrespondingLookupFieldNames.Add(lookup.InternalName);
                    }
                }
            }
        }

        private string CleanViewSchema(string viewXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(viewXml);

            var elem = doc.DocumentElement;
            return elem?.InnerXml;
        }

        private void AnalyzeSecurityConfiguration(ListCreator creator, List list, ClientContext ctx)
        {
            if (list.HasUniqueRoleAssignments)
            {
                ctx.Load(list, l => l.RoleAssignments);
                ctx.Load(list.RoleAssignments, ras => ras.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings));
                ctx.Load(list.ParentWeb, w => w.RoleAssignments);
                ctx.Load(list.ParentWeb.RoleAssignments,
                    ras => ras.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings));
                ctx.ExecuteQueryRetry();

                if (creator.SecurityConfiguration == null)
                {
                    creator.SecurityConfiguration = new SecureObjectCreator
                    {
                        SecureObjectType = SecureObjectType.List,
                        GroupRoleDefinitions = new Dictionary<string, string>()
                    };
                }

                creator.SecurityConfiguration.Title = creator.Title;
                creator.SecurityConfiguration.Url = creator.Url;
                creator.SecurityConfiguration.BreakInheritance = true;

                //First loop thorugh and see if the parent has principals with assignments not found in the list
                CheckShouldCopyExistingPermissions(creator, list);

                //Next loop through the assignments on the list and build the output
                FillListGroupRoleDefinitions(creator, list, ctx);
            }
        }

        private static void CheckShouldCopyExistingPermissions(ListCreator creator, List list)
        {
            creator.SecurityConfiguration.CopyExisting = true;
            foreach (var roleAssignment in list.ParentWeb.RoleAssignments)
            {
                var principal = roleAssignment.Member;
                if (principal.PrincipalType == PrincipalType.SharePointGroup)
                {
                    var foundMatch = false;
                    foreach (var listRoleAssignment in list.RoleAssignments)
                    {
                        if (listRoleAssignment.Member.Id == roleAssignment.Member.Id)
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                    //The first unique ancestor has one that isn't in the list, assume break inheritance
                    if (!foundMatch)
                    {
                        creator.SecurityConfiguration.CopyExisting = false;
                        break;
                    }
                }
            }
        }

        private static void FillListGroupRoleDefinitions(ListCreator creator, List list, ClientContext ctx)
        {
            ctx.Load(ctx.Web.AssociatedMemberGroup, g => g.Id);
            ctx.Load(ctx.Web.AssociatedOwnerGroup, g => g.Id);
            ctx.Load(ctx.Web.AssociatedVisitorGroup, g => g.Id);
            ctx.ExecuteQueryRetry();

            foreach (var roleAssignment in list.RoleAssignments)
            {
                var principal = roleAssignment.Member;
                var principalName = principal.LoginName;
                var principalId = principal.Id;

                if (principalId == ctx.Web.AssociatedMemberGroup.Id)
                {
                    principalName = "AssociatedMemberGroup";
                }
                if (principalId == ctx.Web.AssociatedOwnerGroup.Id)
                {
                    principalName = "AssociatedOwnerGroup";
                }
                if (principalId == ctx.Web.AssociatedVisitorGroup.Id)
                {
                    principalName = "AssociatedVisitorGroup";
                }

                if (principal.PrincipalType == PrincipalType.SharePointGroup ||
                    principal.PrincipalType == PrincipalType.SecurityGroup)
                {
                    foreach (var roleDefinition in roleAssignment.RoleDefinitionBindings)
                    {
                        //This part of the object model is quirky
                        //There should be at most two for a given principal
                        //butif there are more the first one that isn't Limited Access wins
                        if (roleDefinition.Name != "Limited Access")
                        {
                            if (!creator.SecurityConfiguration.CopyExisting)
                            {
                                if (!creator.SecurityConfiguration.GroupRoleDefinitions.ContainsKey(principalName))
                                {
                                    creator.SecurityConfiguration.GroupRoleDefinitions.Add(principalName,
                                        roleDefinition.Name);
                                }
                            }
                            else
                            {
                                var inFirstAncestor = false;
                                foreach (var parentWebRoleAssignment in list.ParentWeb.RoleAssignments)
                                {
                                    if (parentWebRoleAssignment.Member.LoginName == principalName)
                                    {
                                        foreach (
                                            var parentWebRoleDefinition in
                                                parentWebRoleAssignment.RoleDefinitionBindings)
                                        {
                                            if (roleDefinition.Name == parentWebRoleDefinition.Name)
                                            {
                                                inFirstAncestor = true;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                if (!inFirstAncestor)
                                {
                                    if (!creator.SecurityConfiguration.GroupRoleDefinitions.ContainsKey(principalName))
                                    {
                                        creator.SecurityConfiguration.GroupRoleDefinitions.Add(principalName,
                                            roleDefinition.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AnalyzeContentTypes(ListCreator creator, List list)
        {
            var defaultContentTypeId = GetDefaultContentTypeForListTemplate(list.BaseTemplate);
            if (!string.IsNullOrEmpty(defaultContentTypeId) &&
                list.ContentTypes[0].Parent.Id.StringValue != defaultContentTypeId)
            {
                creator.ReplaceDefaultType = true;
            }
            if (list.ContentTypes.Count > 1)
            {
                creator.AdditionalContentTypes = new List<string>();
                for (var i = 1; i < list.ContentTypes.Count; i++)
                {
                    if (!list.ContentTypes[i].Id.StringValue.StartsWith("0x0120")) //Folder
                    {
                        creator.AdditionalContentTypes.Add(list.ContentTypes[i].Name);
                    }
                }
            }
        }

        private void AnalyzeListField(Field field, ListCreator listCreator, ContentTypeCollection ctypes,
            ClientContext ctx)
        {
            CheckIsRequiredField(listCreator, field);
            CheckIsIndexedField(listCreator, field);
            CheckIsHiddenFormOrDisplayOnlyField(listCreator, field);
            CheckIsEnforceUniqueValueField(listCreator, field);
            CheckIsDisplayNameOverideOrAdditionalField(listCreator, field, ctypes, ctx);
        }

        private void CheckIsDisplayNameOverideOrAdditionalField(ListCreator listCreator, Field field,
            ContentTypeCollection ctypes, ClientContext ctx)
        {
            foreach (var ctype in ctypes)
            {
                foreach (var ctypeField in ctype.Parent.Fields)
                {
                    if (ctypeField.Id == field.Id)
                    {
                        if (field.Title == ctypeField.Title) return;
                        if (ctypeField.Title == "Title")
                        {
                            listCreator.TitleFieldDisplayName = field.Title;
                        }
                        //Ignore LinkTitle, etc...
                        if (field.Title != "Title")
                        {
                            listCreator.FieldDisplayNameOverrides.Add(ctypeField.Id.ToString(), field.Title);
                        }
                        return;
                    }
                }
            }
            //TODO: More precise match against the Type attribute for WorkflowStatus
            if (field.FromBaseType || field.SchemaXml.Contains("WorkflowStatus")) return;
            //The field is not in any of the list content types
            try
            {
                var schemaXml = FieldTokenizer.DoTokenSubstitutionsAndCleanSchema(ctx, field);
                var displayName = schemaXml.GetXmlAttribute("DisplayName");
                schemaXml = schemaXml.SetXmlAttribute("Name", displayName.Replace(" ", "_x0020_"));
                listCreator.AdditionalFields.Add(field.InternalName, schemaXml);
            }
            catch
            {
                // ignored
            }
        }

        private void CheckIsHiddenFormOrDisplayOnlyField(ListCreator listCreator, Field field)
        {
            var schemaXml = field.SchemaXml;

            var showDisplay = GetXmlAttributeValue(schemaXml, "ShowInDisplayForm");
            var showEdit = GetXmlAttributeValue(schemaXml, "ShowInEditForm");
            var showNew = GetXmlAttributeValue(schemaXml, "ShowInNewForm");

            if (!string.IsNullOrEmpty(showDisplay) && showDisplay.ToLowerInvariant() == "false" &&
                !string.IsNullOrEmpty(showEdit) && showEdit.ToLowerInvariant() == "false" &&
                !string.IsNullOrEmpty(showNew) && showNew.ToLowerInvariant() == "false")
            {
                listCreator.HiddenFormFields.Add(field.InternalName);
            }
            else if ((string.IsNullOrEmpty(showDisplay) || showDisplay.ToLowerInvariant() == "true") &&
                     !string.IsNullOrEmpty(showEdit) && showEdit.ToLowerInvariant() == "false" &&
                     !string.IsNullOrEmpty(showNew) && showNew.ToLowerInvariant() == "false")
            {
                listCreator.DisplayFormOnlyFields.Add(field.InternalName);
            }
        }

        private void CheckIsRequiredField(ListCreator listCreator, Field field)
        {
            if (field.Required)
            {
                listCreator.RequiredFields = listCreator.RequiredFields ?? new List<string>();
                listCreator.RequiredFields.Add(field.InternalName);
            }
        }

        private void CheckIsIndexedField(ListCreator listCreator, Field field)
        {
            if (field.Indexed)
            {
                listCreator.IndexFields.Add(field.InternalName);
            }
        }

        private void CheckIsEnforceUniqueValueField(ListCreator listCreator, Field field)
        {
            if (field.EnforceUniqueValues)
            {
                listCreator.EnforceUniqueFields.Add(field.InternalName);
            }
        }

        private string GetXmlAttributeValue(string xml, string attribute)
        {
            XDocument document;
            using (var s = new StringReader(xml))
            {
                document = XDocument.Load(s);
            }

            var element = document.Root;
            if (element?.Attribute(attribute) == null) return null;
            return (string)element.Attribute(attribute);
        }

        private string GetDefaultContentTypeForListTemplate(int templateTypeId)
        {
            switch (templateTypeId)
            {
                case (int)ListTemplateType.GenericList:
                    return "0x01"; //Item	
                case (int)ListTemplateType.DocumentLibrary:
                    return "0x0101"; //Document
                case (int)ListTemplateType.Links:
                    return "0x0105"; //Link
                case (int)ListTemplateType.Announcements:
                    return "0x0104"; //Announcement
                case (int)ListTemplateType.Contacts:
                    return "0x0106"; //Contact
                case (int)ListTemplateType.Events:
                    return "0x0102"; //Event
                case (int)ListTemplateType.Tasks:
                case (int)ListTemplateType.TasksWithTimelineAndHierarchy:
                    return "0x0108"; //Task
                case (int)ListTemplateType.DiscussionBoard:
                    return "0x012002";
                case (int)ListTemplateType.PictureLibrary:
                    return "0x010102"; //Picture
                case (int)ListTemplateType.XMLForm:
                    return "0x010101"; //XMLForm
                case (int)ListTemplateType.NoCodeWorkflows:
                    return "0x010107"; //NoCodeWorkflow
                case (int)ListTemplateType.WorkflowProcess:
                    return "0x01"; //Item
                case (int)ListTemplateType.WebPageLibrary:
                    return "0x010108"; //WikiPage
                case (int)ListTemplateType.CustomGrid:
                    return "0x01"; //Item
                case (int)ListTemplateType.DataConnectionLibrary:
                    return "0x010100629D00608F814dd6AC8A86903AEE72AA";
                case (int)ListTemplateType.WorkflowHistory:
                    return "0x0109";
                case (int)ListTemplateType.GanttTasks:
                    return "0x0108";
                case (int)ListTemplateType.HelpLibrary:
                    return "0x0101002BC33ABE0D8E4c16869C931114180652";
                case (int)ListTemplateType.MaintenanceLogs:
                    return "0x01009be2ab5291bf4c1a986910bd278e4f18";
                case (int)ListTemplateType.Posts:
                    return "0x0110";
                case (int)ListTemplateType.Comments:
                    return "0x0111";
                case (int)ListTemplateType.Categories:
                    return "0x01";
                case (int)ListTemplateType.Whereabouts:
                    return "0x0100fbeee6f0c500489b99cda6bb16c398f7";
                case (int)ListTemplateType.CallTrack:
                    return "0x0100807fbac5eb8a4653b8d24775195b5463";
                case (int)ListTemplateType.Circulation:
                    return "0x01000f389e14c9ce4ce486270b9d4713a5d6";
                case (int)ListTemplateType.Timecard:
                    return "0x0100c30dda8edb2e434ea22d793d9ee42058";
                case (int)ListTemplateType.IMEDic:
                    return "0x010018f21907ed4e401cb4f14422abc65304";
                case (int)ListTemplateType.MySiteDocumentLibrary:
                    return "0x0101";
                case (int)ListTemplateType.IssueTracking:
                    return "0x0103";
            }
            return null;
        }

        public List<ListItemCreator> GetListItems(ClientContext ctx, Web web, string listName)
        {
            List list = web.Lists.GetByTitle(listName);

            CamlQuery query = CamlQuery.CreateAllItemsQuery(2000);

            ListItemCollection items = list.GetItems(query);

            //{67df98f4-9dec-48ff-a553-29bece9c5bf4} is Attachments
            var attachmentsFieldId = Guid.Parse("{67df98f4-9dec-48ff-a553-29bece9c5bf4}");

            //ContentTypeId is hidden
            ctx.Load(list.Fields, fields => fields.Where(f => (!f.Hidden && !f.ReadOnlyField && f.Id != attachmentsFieldId) || f.InternalName == "ContentTypeId"));
            ctx.Load(items, itemCol => itemCol.Include(item => item.FieldValuesForEdit, item => item.FieldValuesAsText, item => item.FieldValuesAsHtml, item => item.ContentType));
            ctx.ExecuteQueryRetry();
            var listItems = new List<ListItemCreator>();

            var listItemCreatorBuilder = new ListItemCreatorBuilder(ctx, web, list);

            foreach (ListItem listItem in items)
            {
                var itemCreator = listItemCreatorBuilder.GetListItemCreator(listItem);
                listItems.Add(itemCreator);
            }
            return listItems;
        }
    }
}