using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.SharePoint.Client;
using SharePointUtility;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ListCreator
    {
        public virtual Dictionary<string, string> ListViewSchemas { get; set; }
        public virtual SecureObjectCreator SecurityConfiguration { get; set; }
        public virtual string DocumentTemplateUrl { get; set; }
        public virtual string DefaultViewTitle { get; set; }
        public virtual string DefaultViewSchemaXml { get; set; }
        public virtual string Title { get; set; }
        public virtual string Id { get; set; }
        public virtual string TitleFieldDisplayName { get; set; }
        public virtual List<string> RequiredFields { get; set; }
        public virtual List<string> IndexFields { get; set; }
        public virtual List<string> HiddenFormFields { get; set; }
        public virtual List<string> DisplayFormOnlyFields { get; set; }
        public virtual List<string> EnforceUniqueFields { get; set; }
        public virtual List<string> RemoveViewFields { get; set; }
        public virtual List<string> AddToAllViewsFields { get; set; }
        public virtual Dictionary<string, string> FieldDisplayNameOverrides { get; set; }
        public virtual List<string> AdditionalContentTypes { get; set; }
        public virtual Dictionary<string, string> AdditionalFields { get; set; }
        public virtual bool NoCrawl { get; set; }
        public virtual bool Hidden { get; set; }
        public virtual QuickLaunchOptions QuickLaunchOption { get; set; }
        public virtual Guid TemplateFeatureId { get; set; }
        public virtual int TemplateType { get; set; }
        public virtual string Url { get; set; }
        public virtual string Description { get; set; }
        public virtual bool ReplaceDefaultType { get; set; }
        public virtual string ContentType { get; set; }
        public virtual List<ListItemCreator> ListItems { get; set; }
        public virtual bool DeleteExistingListItems { get; set; }
        public virtual bool ProvisionListItems { get; set; }
        public virtual Dictionary<string, WorkflowAssociationCreator> WorkflowAssociations { get; set; }
        public List List { get; set; }
        public ListInfo ListInfo { get; set; }
        public virtual string CorrespondingLookupFieldName { get; set; }
        public virtual List<string> CorrespondingLookupFieldNames { get; set; }
        public virtual Field CorrespondingLookupField { get; set; }
        public virtual Dictionary<string, CustomActionCreator> CustomActionCreators { get; set; }

        /// <summary>
        /// Property bag entries for the list
        /// </summary>
        public virtual Dictionary<string, string> PropertyBagItems { get; set; } = new Dictionary<string, string>();

        public virtual List<RemoteEventRegistrationCreator> RemoteEventRegistrationCreators { get; set; }

        public virtual void ConfigureBeforeContentTypeBinding(ClientContext ctx)
        {
            if (CorrespondingLookupFieldName != null)
            {
                CorrespondingLookupField = ctx.Web.Fields.GetByInternalNameOrTitle(CorrespondingLookupFieldName);
                Utility.AttachListToLookup(ctx, CorrespondingLookupField, List);
            }
            if (CorrespondingLookupFieldNames != null && CorrespondingLookupFieldNames.Count > 0)
            {
                foreach (var fieldName in CorrespondingLookupFieldNames)
                {
                    var fieldToAttach = ctx.Web.Fields.GetByInternalNameOrTitle(fieldName);
                    Utility.AttachListToLookup(ctx, fieldToAttach, List);
                }
            }
            if (AdditionalFields != null)
            {
                var excludeFields = new List<string>();

                ctx.Load(List.Fields, f => f.Include
                    (field => field.InternalName, field => field.Id));
                ctx.ExecuteQueryRetry();
                foreach (var existingField in List.Fields)
                {
                    //TODO: Change completely to ID from builder down
                    //This is a temporary hack to deal with crazy mechanics 
                    foreach (var field in AdditionalFields)
                    {
                        if (field.Value.ToLower().Contains(existingField.Id.ToString().ToLower()) &&
                            !excludeFields.Contains(field.Key))
                        {
                            excludeFields.Add(field.Key);
                        }
                    }
                }

                foreach (var field in AdditionalFields.Keys)
                {
                    if (!excludeFields.Contains(field))
                    {
                        CleanupTaxonomyHiddenField(ctx, AdditionalFields[field], excludeFields);
                        AddFieldAsXml(field, FieldTokenizer.DoTokenReplacement(ctx, AdditionalFields[field]));
                    }
                }
                ctx.ExecuteQueryRetry();
            }
        }

        private void CleanupTaxonomyHiddenField(ClientContext ctx, string field, List<string> excludeFields)
        {
            var fieldType = field.GetXmlAttribute("Type");
            if (fieldType.StartsWith("TaxonomyField"))
            {
                try
                {
                    var fieldId = Guid.Parse(field.GetXmlAttribute("ID")).ToString("N");

                    Field deleteNoteField = null;
                    if (excludeFields.Contains(fieldId))
                    {
                        deleteNoteField = List.Fields.GetByInternalNameOrTitle(fieldId);
                        deleteNoteField.DeleteObject();
                        ctx.ExecuteQuery();
                    }
                    else
                    {
                        var noteDisplayName = $"{field.GetXmlAttribute("Name")}_0";
                        deleteNoteField = List.Fields.GetByTitle(noteDisplayName);
                        deleteNoteField.DeleteObject();
                        ctx.ExecuteQuery();
                    }
                }
                catch
                {
                   //Ignore
                }
            }
        }

        public virtual void ConfigureFieldsAndViews(ClientContext ctx)
        {
            RefreshList(ctx);

            if ((IndexFields != null && IndexFields.Count > 0) || (RequiredFields != null && RequiredFields.Count > 0) ||
                EnforceUniqueFields != null && EnforceUniqueFields.Count > 0)
            {
                if (RequiredFields != null && RequiredFields.Count > 0)
                {
                    foreach (var field in RequiredFields)
                    {
                        Utility.RequireField(List, field);
                    }
                }

                if (IndexFields != null && IndexFields.Count > 0)
                {
                    foreach (var field in IndexFields)
                    {
                        Utility.IndexField(List, field);
                    }
                }

                if (EnforceUniqueFields != null && EnforceUniqueFields.Count > 0)
                {
                    foreach (var field in EnforceUniqueFields)
                    {
                        Utility.EnforceUniqueField(List, field);
                    }
                }
                ctx.ExecuteQueryRetry();
                RefreshList(ctx);
            }
            if (HiddenFormFields != null && HiddenFormFields.Count > 0)
            {
                foreach (var fieldName in HiddenFormFields)
                {
                    Utility.HideFieldOnAllForms(List, fieldName);
                }
                ctx.ExecuteQueryRetry();
                RefreshList(ctx);
            }
            if (DisplayFormOnlyFields != null && DisplayFormOnlyFields.Count > 0)
            {
                foreach (var field in DisplayFormOnlyFields)
                {
                    Utility.ShowOnDisplayFormOnly(List, field);
                }
                ctx.ExecuteQueryRetry();
                RefreshList(ctx);
            }
            if (!string.IsNullOrEmpty(TitleFieldDisplayName))
            {
                Utility.SetTitleFieldDisplayName(List, TitleFieldDisplayName);
                ctx.ExecuteQueryRetry();
                RefreshList(ctx);
            }
            if (FieldDisplayNameOverrides != null && FieldDisplayNameOverrides.Count > 0)
            {
                foreach (var field in FieldDisplayNameOverrides.Keys)
                {
                    Utility.SetFieldDisplayName(List, field, FieldDisplayNameOverrides[field]);
                }
                ctx.ExecuteQueryRetry();
                RefreshList(ctx);
            }

            if (!string.IsNullOrEmpty(DefaultViewSchemaXml) || !string.IsNullOrEmpty(DefaultViewTitle))
            {
                var defaultView = List.Views[0];
                if (!string.IsNullOrEmpty(DefaultViewSchemaXml))
                {
                    defaultView.ListViewXml = DefaultViewSchemaXml;
                }
                if (!string.IsNullOrEmpty(DefaultViewTitle))
                {
                    defaultView.Title = DefaultViewTitle;
                }
                defaultView.Update();
                ctx.ExecuteQueryRetry();
            }

            if (ListViewSchemas != null)
            {
                foreach (var key in ListViewSchemas.Keys)
                {
                    if (!ViewExists(key))
                    {
                        var vcInfo = new ViewCreationInformation {Title = key};
                        var view = List.Views.Add(vcInfo);
                        view.ListViewXml = ListViewSchemas[key];
                        view.Update();
                    }
                    else
                    {
                        var view = List.Views.GetByTitle(key);
                        view.ListViewXml = ListViewSchemas[key];
                        view.Update();
                    }
                }
                ctx.ExecuteQueryRetry();
            }
            if (RemoveViewFields != null || AddToAllViewsFields != null)
            {
                foreach (var view in List.Views)
                {
                    if (RemoveViewFields != null)
                    {
                        foreach (var field in RemoveViewFields)
                        {
                            if (view.ViewFields.SchemaXml.Contains(field)) view.ViewFields.Remove(field);
                        }
                    }
                    if (AddToAllViewsFields != null)
                    {
                        foreach (var field in AddToAllViewsFields)
                        {
                            if (!view.ViewFields.SchemaXml.Contains(field)) view.ViewFields.Add(field);
                        }
                    }
                    view.Update();
                }
                ctx.ExecuteQueryRetry();
            }
        }

        private void RefreshList(ClientContext ctx)
        {
            //Reload fields
            //TODO: This is a hack to fix a bug in server version 16.0.3417.1200
            //Dump the List object and reload it
            var refreshedList = ctx.Web.Lists.GetByTitle(List.Title);
            var hackWorked = false;
            var retryCounter = 0;
            do
            {
                ctx.Load(refreshedList, l => l.Id, l => l.Title);
                ctx.Load(refreshedList.Fields, f => f.Include
                    (field => field.InternalName, field => field.SchemaXml));
                ctx.Load(refreshedList.Views,
                    v => v.Include
                        (view => view.Id, view => view.ViewFields, view => view.Title));
                try
                {
                    ctx.ExecuteQueryRetry();
                    hackWorked = true;
                }
                catch
                {
                    retryCounter++;
                    Trace.WriteLine("Query for " + List.Title + " failed");
                    if (retryCounter > 4) throw;
                }
            } while (!hackWorked);
            //End Hack

            List = refreshedList;
        }

        public virtual void FinalizeConfiguration(ClientContext ctx)
        {
            if (!string.IsNullOrEmpty(ContentType)) ListInfo = new ListInfo(List, ContentType);
            if (NoCrawl || Hidden)
            {
                List.NoCrawl = NoCrawl;
                List.Hidden = Hidden;
                List.Update();
            }
            if (CustomActionCreators != null && CustomActionCreators.Count > 0)
            {
                var customActionManager = new CustomActionManager {CustomActions = CustomActionCreators};
                customActionManager.CreateAll(ctx, List);
            }
        }

        public virtual void UpdateDocumentTemplate(ClientContext ctx)
        {
            if (DocumentTemplateUrl != null)
            {
                List = ctx.Web.Lists.GetByTitle(Title);
                List.ContentTypesEnabled = true;
                List.Update();
                ctx.Load(List.ContentTypes);
                ctx.ExecuteQueryRetry();
                List.ContentTypes[0].DocumentTemplate = DocumentTemplateUrl.Replace("{@WebServerRelativeUrl}",
                    ctx.Web.ServerRelativeUrl);
                List.ContentTypes[0].Update(false);
                ctx.ExecuteQueryRetry();
            }
        }

        protected void AddFieldAsXml(string name, string xml)
        {
            var safeToAdd = true;
            if (List.IsObjectPropertyInstantiated("Fields"))
            {
                foreach (var field in List.Fields)
                {
                    if (field.IsPropertyAvailable("InternalName"))
                    {
                        if (field.InternalName == name)
                        {
                            safeToAdd = false;
                            break;
                        }
                    }
                }
            }
            if (safeToAdd)
            {
                List.Fields.AddFieldAsXml(xml, false, AddFieldOptions.AddToAllContentTypes);
            }
        }

        protected bool ViewExists(string title)
        {
            foreach (var view in List.Views)
            {
                if (view.IsPropertyAvailable("Title") && view.Title == title)
                {
                    return true;
                }
            }
            return false;
        }
    }
}