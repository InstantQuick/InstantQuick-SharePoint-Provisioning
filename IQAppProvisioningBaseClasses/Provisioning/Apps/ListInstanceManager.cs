using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using SharePointUtility;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ListInstanceManager : ProvisioningManagerBase
    {
        private readonly ClientContext _ctx;
        private readonly bool _isHostWeb;
        private readonly Web _web;
        private Dictionary<string, ContentType> _existingContentTypes;

        //TODO: Create add lookup columns functionality
        private Dictionary<string, List> _existingLists;

        public ListInstanceManager()
        {
        }

        public ListInstanceManager(ClientContext ctx, bool isHostWeb) : this(ctx, ctx.Web, isHostWeb)
        {
        }

        public ListInstanceManager(ClientContext ctx, Web web, bool isHostWeb)
        {
            _isHostWeb = isHostWeb;
            _ctx = ctx;
            _web = web;
        }

        public virtual Dictionary<string, ListCreator> Creators { get; set; }


        public void CreateAll()
        {
            try
            {
                _existingLists = new Dictionary<string, List>();
                _existingContentTypes = new Dictionary<string, ContentType>();
                GetAllListsAndContentTypes();
                CreateLists();
                ConfigureBeforeContentTypeBinding();
                BindContentTypes();
                ConfigureFieldsAndViews();
                ApplySecurity();
                FinalizeConfigurations();
                AddAndDeleteListItems();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error provisioning lists " + _web.Url + " | " + ex);
                throw;
            }
        }

        private void AddAndDeleteListItems()
        {
            var batchSize = 100;
            var creatorsWithItems =
                Creators.Values.Where(l => l.ProvisionListItems && l.ListItems != null && l.ListItems.Count > 0).ToList();

            var creatorsWithListItemsToClear = Creators.Values.Where(l => l.DeleteExistingListItems).ToList();

            if (creatorsWithListItemsToClear.Count > 0)
            {
                DeleteExistingItems(creatorsWithListItemsToClear, batchSize);
            }

            if (creatorsWithItems.Count == 0) return;

            var creatorsWithLookups =
                creatorsWithItems.Where(
                    c =>
                        c.ListItems.FirstOrDefault(i => i.FieldValues.FirstOrDefault(li => li.FieldType == "Lookup") != null) != null).ToList();

            List<ListCreator> sortedCreators;
            Dictionary<string, Dictionary<string, string[]>> lookupQueries = new Dictionary<string, Dictionary<string, string[]>>();

            if (creatorsWithLookups.Count == 0)
            {
                sortedCreators = creatorsWithItems;
            }
            else
            {
                sortedCreators = new List<ListCreator>();

                //Add the ones with no lookup fields first, it doesn't matter what order they go in
                foreach (var creatorWithItems in creatorsWithItems)
                {
                    if (!creatorsWithLookups.Contains(creatorWithItems))
                    {
                        sortedCreators.Add(creatorWithItems);
                    }
                }

                //Note that this approach doesn't deal with circular relationships,
                //but there's not really much that can be done in that case other than make mutliple passes
                //so, if you need it, you'll need to write it!
                foreach (var creatorWithLookups in creatorsWithLookups)
                {
                    var lookupFields = creatorWithLookups.List.Fields.Where(f => creatorWithLookups.ListItems[0].FieldValues.FirstOrDefault(li => li.FieldName == f.InternalName && li.FieldType == "Lookup") != null);
                    foreach (var field in lookupFields)
                    {
                        var lookupField = _ctx.CastTo<FieldLookup>(field);
                        lookupField.EnsureProperties(lf => lf.LookupList, lf => lf.LookupField);
                        var lookupListId = lookupField.LookupList;
                        var lookupList = _web.Lists.GetById(Guid.Parse(lookupListId));
                        _ctx.Load(lookupList, l => l.Title);
                        try
                        {
                            _ctx.ExecuteQueryRetry();
                        }
                        catch
                        {
                            //Ignore
                        }
                        if (Creators.ContainsKey(lookupList.Title) && !sortedCreators.Contains(Creators[lookupList.Title]) && creatorsWithItems.Contains(Creators[lookupList.Title]))
                        {
                            sortedCreators.Add(Creators[lookupList.Title]);
                        }

                        if (!lookupQueries.ContainsKey(creatorWithLookups.Title))
                        {
                            lookupQueries[creatorWithLookups.Title] = new Dictionary<string, string[]>();
                        }

                        lookupQueries[creatorWithLookups.Title][field.InternalName] = new string[]
                        {
                            lookupList.Title,
                            $"<View><ViewFields><FieldRef Name='{lookupField.LookupField}'/></ViewFields><Query><Where><Contains><FieldRef Name='{lookupField.LookupField}' /><Value Type='Text'>#VALUE#</Value></Contains></Where></Query><RowLimit>1</RowLimit></View>"
                        };
                    }
                    sortedCreators.Add(creatorWithLookups);
                }
            }

            TaxonomySession taxonomySession = null;
            TermStore termStore = null;

            ClientContext tempCtx = _ctx.Clone(_web.Url);
            if (sortedCreators.FirstOrDefault(c => c.ListItems.FirstOrDefault(li => li.FieldValues.FirstOrDefault(fv => fv.FieldType.StartsWith("TaxonomyField")) != null) != null) != null)
            {
                taxonomySession = TaxonomySession.GetTaxonomySession(tempCtx);
                termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            }

            foreach (var creator in sortedCreators)
            {
                if (creator.ProvisionListItems && creator.ListItems != null)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Creating items for " + creator.Title);
                    creator.List.ContentTypes.EnsureProperties(cts => cts.Include(ct => ct.Id, ct => ct.Name));
                    var specialTypes = new List<string>()
                    {
                        "Lookup",
                        "DateTime",
                        "User",
                        "TaxonomyFieldType",
                        "TaxonomyFieldTypeMulti"
                    };

                    var specialNames = new List<string>()
                    {
                        "ContentTypeId",
                        "ContentType"
                    };

                    for (var i = 0; i < creator.ListItems.Count; i++)
                    {
                        var newItemInfo = creator.ListItems[i];
                        var item = creator.List.AddItem(new ListItemCreationInformation());
                        foreach (var fieldInfo in newItemInfo.FieldValues)
                        {
                            if (!specialNames.Contains(fieldInfo.FieldName) &&
                                !specialTypes.Contains(fieldInfo.FieldType))
                            {
                                item[fieldInfo.FieldName] = fieldInfo.Value.Replace("{@WebUrl}", _web.Url).Replace("{@WebServerRelativeUrl}", _web.ServerRelativeUrl);
                            }
                            else if (fieldInfo.FieldType == "DateTime")
                            {
                                item[fieldInfo.FieldName] = DateTime.ParseExact(fieldInfo.Value, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
                            }
                            else if (fieldInfo.FieldType == "User")
                            {
                                try
                                {
                                    var user = tempCtx.Web.EnsureUser(fieldInfo.Value);
                                    tempCtx.Load(user);
                                    tempCtx.ExecuteQueryRetry();
                                    item[fieldInfo.FieldName] = user.Id;
                                }
                                catch
                                {
                                    OnNotify(ProvisioningNotificationLevels.Verbose, $"Unable to ensure user {fieldInfo.Value}. Adding the list item, but leaving the field blank!");
                                }
                            }
                            else if (fieldInfo.FieldType == "Lookup")
                            {
                                if (lookupQueries.ContainsKey(creator.Title) && lookupQueries[creator.Title].ContainsKey(fieldInfo.FieldName))
                                {
                                    var lookupListTitle = lookupQueries[creator.Title][fieldInfo.FieldName][0];
                                    var viewXml = lookupQueries[creator.Title][fieldInfo.FieldName][1].Replace(
                                        "#VALUE#", fieldInfo.Value);

                                    var lookupList = tempCtx.Web.Lists.GetByTitle(lookupListTitle);
                                    var query = new CamlQuery();
                                    query.ViewXml = viewXml;
                                    var listItems = lookupList.GetItems(query);
                                    tempCtx.Load(listItems);
                                    try
                                    {
                                        tempCtx.ExecuteQueryRetry();
                                        if (listItems.Count == 0)
                                        {
                                            OnNotify(ProvisioningNotificationLevels.Verbose, $"Unable to find {fieldInfo.Value} in {lookupListTitle}. Skipping attempt to set list item value.");
                                        }
                                        else
                                        {
                                            item[fieldInfo.FieldName] = listItems[0].Id;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnNotify(ProvisioningNotificationLevels.Verbose, $"Unable to find {fieldInfo.Value} in {lookupListTitle}. An exception occurred: {ex.Message}. Skipping attempt to set list item value.");
                                    }
                                }
                                else
                                {
                                    OnNotify(ProvisioningNotificationLevels.Verbose, $"Bad data for lookup field {fieldInfo.FieldName}. Skipping attempt to set list item value.");
                                }

                            }
                            else if (fieldInfo.FieldType.StartsWith("TaxonomyField"))
                            {

                                var termSetName = fieldInfo.Value.GetInnerText("{@TermSet:", "}");
                                var termNames = fieldInfo.Value.GetInnerText("{@Terms:", "}").Split(';');
                                var termSets = termStore.GetTermSetsByName(termSetName, (int)_web.Language);
                                tempCtx.Load(termSets, ts => ts.Include(t => t.Id));
                                tempCtx.ExecuteQueryRetry();

                                if (termSets.Count == 0)
                                {
                                    OnNotify(ProvisioningNotificationLevels.Verbose,
                                        $"Unable to find term set {termSetName}. Skipping list item field!");
                                }
                                else if (termNames.Length == 0)
                                {
                                    OnNotify(ProvisioningNotificationLevels.Verbose,
                                        $"Bad field value token {fieldInfo.Value}. Skipping list item field!");
                                }
                                else
                                {
                                    var terms = termSets[0].GetAllTerms();
                                    tempCtx.Load(terms);
                                    tempCtx.ExecuteQueryRetry();

                                    var fieldValue = string.Empty;
                                    for (var c = 0; c < termNames.Length; c++)
                                    {
                                        var termName = termNames[c];

                                        var foundTerm = terms.FirstOrDefault(t => t.Name == termName);

                                        if (foundTerm == null)
                                        {
                                            OnNotify(ProvisioningNotificationLevels.Verbose, $"Unable to find term {termName}. Skipping list item field!");
                                            break;
                                        }

                                        if (fieldValue != String.Empty)
                                        {
                                            fieldValue = fieldValue + ";";
                                        }
                                        if (termNames.Length == 1)
                                        {
                                            fieldValue = $"-1;#{termName}|{foundTerm.Id}";
                                        }
                                        else
                                        {
                                            fieldValue = fieldValue + $"{termName}|{foundTerm.Id}";
                                        }
                                    }
                                    if (fieldValue != String.Empty)
                                    {
                                        item[fieldInfo.FieldName] = fieldValue;
                                    }
                                }
                            }
                            else if (fieldInfo.FieldName == "ContentType" &&
                                     creator.ContentType.ToLowerInvariant() != fieldInfo.Value.ToLowerInvariant())
                            {

                                var itemCType =
                                    creator.List.ContentTypes.FirstOrDefault(ctype => ctype.Name == fieldInfo.Value);
                                if (itemCType == null)
                                {
                                    throw new InvalidOperationException(
                                        $"Content type {fieldInfo.Value} not found in list. Unable to add item.");
                                }
                                else
                                {
                                    item["ContentTypeId"] = itemCType.Id;
                                }
                            }
                        }
                        item.Update();
                        if (i % batchSize == 0)
                        {
                            _ctx.ExecuteQueryRetry();
                        }
                    }
                    _ctx.ExecuteQueryRetry();
                }
            }
        }

        private void DeleteExistingItems(List<ListCreator> creatorsWithListItemsToClear, int batchSize)
        {
            foreach (var creator in creatorsWithListItemsToClear)
            {
                if (creator.DeleteExistingListItems)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Deleting items from " + creator.Title);

                    var query = @"<View><ViewFields><FieldRef Name='ID' /></ViewFields></View>";

                    var view = new CamlQuery { ViewXml = query };
                    var existingItems = creator.List.GetItems(view);
                    _ctx.Load(existingItems);
                    _ctx.ExecuteQueryRetry();

                    for (var i = existingItems.Count - 1; i >= 0; i--)
                    {
                        existingItems[i].DeleteObject();
                        if (i % batchSize == 0)
                        {
                            _ctx.ExecuteQueryRetry();
                        }
                    }

                    _ctx.ExecuteQueryRetry();
                }
            }
        }

        private void ApplySecurity()
        {
            if (!_isHostWeb) return;

            var secureObjects = new List<SecureObjectCreator>();
            foreach (var creator in Creators.Values)
            {
                if (creator.SecurityConfiguration != null)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Applying security to " + creator.Title);
                    creator.SecurityConfiguration.SecurableObject = creator.List;
                    creator.SecurityConfiguration.SecureObjectType = SecureObjectType.List;
                    secureObjects.Add(creator.SecurityConfiguration);
                }
            }
            if (secureObjects.Count > 0)
            {
                var secureObjectManager = new SecureObjectManager(_ctx) { SecureObjects = secureObjects };
                secureObjectManager.ApplySecurity();
            }
        }

        public void DeleteAll()
        {
            if (Creators == null) return;
            _existingLists = new Dictionary<string, List>();
            _existingContentTypes = new Dictionary<string, ContentType>();
            GetAllListsAndContentTypes();
            foreach (var creator in Creators.Values)
            {
                if (creator.List != null)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Deleting " + creator.Title);
                    creator.List.DeleteObject();
                }
            }
            _ctx.ExecuteQueryRetry();
        }

        private void GetAllListsAndContentTypes()
        {
            //Get the available content types
            //These are needed for binding
            _ctx.Load(_web.AvailableContentTypes,
                c => c.Include
                    (contentType => contentType.Name));

            //Get all the existing lists
            _ctx.Load(_web.Lists,
                l => l.Include
                    (list => list.Title, list => list.Id));

            _ctx.ExecuteQueryRetry();

            var foundExisting = false;

            foreach (var list in _web.Lists)
            {
                _existingLists[list.Title] = list;


                //If a list to create already exists
                if (Creators != null && Creators.Keys.Contains(list.Title))
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "List already exists " + list.Title);
                    foundExisting = true;

                    //Attach the list to the creator
                    Creators[list.Title].List = list;

                    //Load its fields and content types
                    _ctx.Load(list.Fields,
                        f => f.Include
                            (field => field.InternalName));

                    _ctx.Load(list.ContentTypes,
                        c => c.Include
                            (contentType => contentType.Name));

                    _ctx.Load(list.Views);
                }
            }

            //Execute the query to load the content types and fields of the existing lists
            if (foundExisting)
            {
                _ctx.ExecuteQueryRetry();
            }

            foreach (var contentType in _web.AvailableContentTypes)
            {
                _existingContentTypes[contentType.Name] = contentType;
            }
        }

        private void CreateLists()
        {
            foreach (var key in Creators.Keys)
            {
                var creator = Creators[key];
                if (!_existingLists.Keys.Contains(key))
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Creating list " + key);
                    CreateList(creator);
                    creator.ProvisionListItems = true;
                    _existingLists[key] = Creators[key].List;
                    _ctx.ExecuteQueryRetry();
                }
                else
                {
                    creator.List = _existingLists[key];
                    _ctx.Load(creator.List.ContentTypes,
                        c => c.Include
                            (contentType => contentType.Name));
                    _ctx.ExecuteQueryRetry();
                }
            }
            if (_ctx.HasPendingRequest)
            {
                _ctx.ExecuteQueryRetry();
            }
        }

        private void CreateList(ListCreator creator)
        {
            var info = new ListCreationInformation();

            //Strip leading forward slash
            if (creator.Url.StartsWith("/"))
            {
                creator.Url = creator.Url.Substring(1);
            }

            info.Title = creator.Title;
            info.QuickLaunchOption = creator.QuickLaunchOption;
            info.TemplateFeatureId = creator.TemplateFeatureId;
            info.TemplateType = creator.TemplateType;
            info.Url = creator.Url;
            info.Description = creator.Description;

            creator.List = _web.Lists.Add(info);
            //Get the list's current content types
            _ctx.Load(creator.List.ContentTypes,
                c => c.Include
                    (contentType => contentType.Name));

            _ctx.Load(creator.List, l => l.Id, l => l.Title);
        }

        private void ConfigureBeforeContentTypeBinding()
        {
            foreach (var creator in Creators.Values)
            {
                creator.ConfigureBeforeContentTypeBinding(_ctx);
            }
            _ctx.ExecuteQueryRetry();
        }

        private void BindContentTypes()
        {
            foreach (var creator in Creators.Values)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, "Binding content types to " + creator.Title);

                _ctx.Load(creator.List.Fields,
                    f => f.Include
                        (field => field.SchemaXml, field => field.InternalName));

                _ctx.Load(creator.List.ContentTypes,
                    c => c.Include
                        (ctype => ctype.Name, ctype => ctype.Id));

                _ctx.Load(creator.List.Views,
                    v => v.Include
                        (view => view.Id, view => view.ViewFields, view => view.Title));

                if (creator.ReplaceDefaultType)
                {
                    var notFound = Enumerable.All(creator.List.ContentTypes, t => t.Name != creator.ContentType);

                    //Add a new one and delete the original default.
                    //If the new one isn't there already
                    if (creator.List.ContentTypes[0].Name != creator.ContentType && notFound)
                    {
                        creator.List.ContentTypes.AddExistingContentType(_existingContentTypes[creator.ContentType]);
                        creator.List.ContentTypes[0].DeleteObject();
                        _ctx.ExecuteQueryRetry();
                    }
                    else
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose, "Ensuring fields for " + creator.Title);

                        //Scan the list content type to make sure all the fields are there
                        //and add any that are missing
                        var siteCType = _existingContentTypes[creator.ContentType];
                        var listCType = creator.List.ContentTypes[0];
                        _ctx.Load(siteCType, ct => ct.Fields, ctype => ctype.Name, ctype => ctype.Id);
                        _ctx.Load(listCType, ct => ct.Fields, ctype => ctype.Name, ctype => ctype.Id);
                        _ctx.ExecuteQueryRetry();
                        var updated = false;
                        foreach (var siteField in siteCType.Fields)
                        {
                            var found = false;
                            foreach (var listField in listCType.Fields)
                            {
                                if (listField.Id == siteField.Id)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                creator.List.Fields.Add(siteField);
                                updated = true;
                            }
                        }
                        if (updated) _ctx.ExecuteQueryRetry();
                    }
                }
                if (creator.AdditionalContentTypes != null && creator.AdditionalContentTypes.Count > 0)
                {
                    var existingCTypes = new List<string>();
                    foreach (var existingCType in creator.List.ContentTypes)
                    {
                        if (!existingCType.IsPropertyAvailable("Name"))
                        {
                            _ctx.Load(existingCType, ct => ct.Name);
                            _ctx.ExecuteQueryRetry();
                        }
                        existingCTypes.Add(existingCType.Name);
                    }
                    foreach (var contentType in creator.AdditionalContentTypes)
                    {
                        //Bind it if it is in the site, but not already bound to the list
                        var currentType = existingCTypes.FirstOrDefault(name => name == contentType);
                        if (_existingContentTypes.ContainsKey(contentType) && currentType == null)
                        {
                            _ctx.Load(creator.List.ContentTypes.AddExistingContentType(_existingContentTypes[contentType]), ct => ct.Name);
                        }
                    }
                }
            }
            _ctx.ExecuteQueryRetry();
        }

        private void ConfigureFieldsAndViews()
        {
            foreach (var creator in Creators.Values)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, "Configuring fields and views for " + creator.Title);
                creator.ConfigureFieldsAndViews(_ctx);
            }
        }

        private void FinalizeConfigurations()
        {
            foreach (var creator in Creators.Values)
            {
                if (creator.List != null)
                {
                    creator.FinalizeConfiguration(_ctx);
                }
            }
            _ctx.ExecuteQueryRetry();

            if (_existingLists.Keys.Contains("Settings"))
            {
                var listHelper = ListHelper.FromSettings(_ctx);
                foreach (var creator in Creators.Values)
                {
                    if (creator.List != null)
                    {
                        creator.FinalizeConfiguration(_ctx);
                        if (creator.ListInfo != null)
                        {
                            listHelper.Add(creator.ListInfo);
                        }
                    }
                }
                ListHelper.ToSettings(listHelper, _ctx);
                _ctx.ExecuteQueryRetry();
            }
        }
    }
}