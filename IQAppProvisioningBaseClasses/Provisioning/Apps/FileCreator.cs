using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint.Client.WebParts;
using File = Microsoft.SharePoint.Client.File;
using static IQAppProvisioningBaseClasses.Constants;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class FileCreator
    {
        protected Dictionary<string, Guid> ViewMappings = new Dictionary<string, Guid>();

        //New system fallback for lack of webpartpages.asmx
        protected Dictionary<string, Guid> ViewMappings2 = new Dictionary<string, Guid>();

        protected Dictionary<Guid, View> Views = new Dictionary<Guid, View>();
        public virtual SecureObjectCreator SecurityConfiguration { get; set; }
        public virtual string AppPageIdentifier { get; set; }
        public virtual string Name { get; set; }
        public virtual Dictionary<string, string> ViewSchemas { get; set; }
        public virtual Dictionary<string, ListViewWebPart> ListViewWebParts { get; set; }
        public virtual List<ListItemFieldValue> ListItemFieldValues { get; set; }
        public virtual bool IsHomePage { get; set; }
        public virtual string Url { get; set; }
        public virtual string HostWebResourceKey { get; set; }
        public virtual string List { get; set; }
        public virtual string ContentType { get; set; }
        public string ContentTypeId { get; set; }
        public virtual bool IsBinary { get; set; }
        public virtual bool DeleteOnCleanup { get; set; }
        public virtual bool ForceOverwrite { get; set; }
        public virtual bool Created { get; set; }
        public File File { get; set; }

        /// <summary>
        ///     Represents the local file system relative file path [relative to the app manifest's BaseFilePath property]
        /// </summary>
        public virtual string RelativeFilePath { get; set; }
        public virtual Dictionary<string, string> WebParts { get; set; }
        public virtual List<WebPartZoneMapping> WebPartPageZoneMappings { get; set; }
        public virtual Dictionary<string, string> WebPartPageWebPartListViews { get; set; }

        //Original name is unfortunate as this is also used for publishing pages
        public virtual Dictionary<string, string> WikiPageWebPartStorageKeyMappings { get; set; }

        //Original name is unfortunate as this is also used for publishing pages
        public virtual Dictionary<string, string> WikiPageWebPartListViews { get; set; }


        public virtual byte[] PrepareFile(byte[] file, ClientContext ctx)
        {
            if (!string.IsNullOrEmpty(AppPageIdentifier))
            {
                var fileText = Encoding.UTF8.GetString(file);
                fileText = fileText.Replace(@"{@appPageIdentifier}", AppPageIdentifier);
                file = Encoding.UTF8.GetBytes(fileText);
            }
            return file;
        }

        public byte[] PrepareFile(byte[] file, ClientContext ctx, Web web, bool uppercaseGuids)
        {
            var fileText = Encoding.UTF8.GetString(file);

            var hasAppPageIdentifierToken = fileText.Contains("{@appPageIdentifier}");
            var hasListIdToken = fileText.Contains("{@ListId:");
            var hasListUrlToken = fileText.Contains("{@ListUrl:");
            var hasWebUrl = fileText.Contains("{@WebServerRelativeUrl}") || fileText.Contains("{@WebUrl}");
            var hasListContentType = fileText.Contains("{@ListContentType:");

            //This is a crude way to go about cooercing the version numbers
            //for files created on SPO/SP2016 for SP2013 on prem
            //probably valid 99.999% of the time, but if it isn't for you, sorry!
            if(ctx.ServerLibraryVersion.Major == 15)
            {
                fileText = fileText.Replace("16.0.0.0", "15.0.0.0");
            }

            if (hasAppPageIdentifierToken)
            {
                fileText = fileText.Replace("{@appPageIdentifier}", AppPageIdentifier);
            }
            if (hasListIdToken || hasListUrlToken)
            {
                fileText = ReplaceListTokens(fileText, ctx, web, uppercaseGuids);
            }
            if (hasWebUrl)
            {
                fileText = fileText.Replace("{@WebUrl}", web.Url).Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl);
            }
            if (hasListContentType)
            {
                var tokenValues = fileText.GetInnerTextList("{@ListContentType:", "}");
                foreach (var tokenValue in tokenValues)
                {
                    var fullToken = "{@ListContentType:" + tokenValue + "}";
                    var parts = tokenValue.Split('|');
                    if (parts.Length != 2)
                    {
                        throw new InvalidDataException("ListContentType:" + tokenValue + " isn't the correct format");
                    }
                    var listTitle = parts[0].Trim();
                    var contentTypeName = parts[1].Trim();

                    var list = web.Lists.GetByTitle(listTitle);
                    var ctypes = list.ContentTypes;
                    ctx.Load(ctypes, cts => cts.Include(ct => ct.Name, ct => ct.Id));
                    ctx.ExecuteQueryRetry();

                    foreach (var contentType in ctypes)
                    {
                        if (contentType.Name == contentTypeName)
                        {
                            fileText = fileText.Replace(fullToken, contentType.Id.StringValue);
                        }
                    }
                }
            }
            file = Encoding.UTF8.GetBytes(fileText);
            return file;
        }

        private string ReplaceListTokens(string fileText, ClientContext ctx, Web web, bool uppercaseGuids)
        {
            var listIdTokens = fileText.GetInnerTextList("{@ListId:", "}");
            var listUrlTokens = fileText.GetInnerTextList("{@ListUrl:", "}");

            var matchedLists = new Dictionary<string, List>();

            GetTokenLists(ctx, web, listIdTokens, matchedLists);
            GetTokenLists(ctx, web, listUrlTokens, matchedLists);

            foreach (var listTitle in listIdTokens)
            {
                fileText = fileText.Replace("{@ListId:" + listTitle + "}",
                    uppercaseGuids
                        ? matchedLists[listTitle].Id.ToString().ToUpper()
                        : matchedLists[listTitle].Id.ToString());
            }

            foreach (var listTitle in listUrlTokens)
            {
                fileText = fileText.Replace("{@ListUrl:" + listTitle + "}",
                    matchedLists[listTitle].RootFolder.ServerRelativeUrl);
            }

            return fileText;
        }

        private static void GetTokenLists(ClientContext ctx, Web web, List<string> listTokens,
            Dictionary<string, List> matchedLists)
        {
            foreach (var title in listTokens)
            {
                GetListForToken(ctx, web, matchedLists, title);
            }
        }

        private static void GetListForToken(ClientContext ctx, Web web, Dictionary<string, List> matchedLists,
            string title)
        {
            if (!matchedLists.ContainsKey(title))
            {
                var list = web.Lists.GetByTitle(title);
                ctx.Load(list, l => l.Id, l => l.RootFolder);
                try
                {
                    ctx.ExecuteQueryRetry();
                    matchedLists.Add(title, list);
                }
                catch
                {
                    throw new InvalidOperationException("File depends on " + title + ". List not found");
                }
            }
        }

        public virtual void SetProperties(ClientContext ctx, Web web)
        {
            if (!Created) return;

            if (ListViewWebParts != null && ListViewWebParts.Count > 0)
            {
                foreach (var webPart in ListViewWebParts.Values)
                {
                    if (!webPart.IsCalendar)
                    {
                        AddListViewWebPart(ctx, web, "IQAppProvisioningBaseClasses.ListViewWebParts.BaseListView.webpart",
                            webPart.Title, webPart.ZoneId, webPart.Order, webPart.ListName);
                    }
                    else
                    {
                        AddListViewWebPart(ctx, web, "IQAppProvisioningBaseClasses.ListViewWebParts.Calendar.webpart",
                            webPart.Title, webPart.ZoneId, webPart.Order, webPart.ListName, true);
                    }
                }
            }

            if (ViewSchemas != null && ViewSchemas.Count > 0)
            {
                UpdateViews();
            }

            if (IsHomePage)
            {
                var rootFolder = ctx.Web.RootFolder;
                rootFolder.WelcomePage = Url.Substring(0, 1) == "/" ? Url.Substring(1) : Url;
                rootFolder.Update();
            }

            UpdateListItem(ctx, web);

            ctx.ExecuteQueryRetry();

            try
            {
                File.CheckIn("", CheckinType.MajorCheckIn);
                ctx.ExecuteQueryRetry();
            }
            catch
            {
                //Exceptions expected since we aren't checking to see if the target is configured for checkin
                //Trace.WriteLine(ex);
            }

            try
            {
                File.Publish("");
                ctx.ExecuteQueryRetry();
            }
            catch
            {
                //Exceptions expected since we aren't checking to see if the target is configured for approval
                //Trace.WriteLine(ex);
            }
        }

        private void UpdateListItem(ClientContext ctx, Web web)
        {
            if (ListItemFieldValues == null || ListItemFieldValues.Count == 0) return;

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
                "ContentType",
                WikiPageContentFieldName,      //Should ignore, set elsewhere
                PublishingPageContentFieldName //Should ignore, set elsewhere

            };

            var item = File.ListItemAllFields;
            TaxonomySession taxonomySession = null;
            TermStore termStore = null;
            ClientContext tempCtx = ctx.Clone(web.Url);
            var library = tempCtx.Web.Lists.GetByTitle(List);
            tempCtx.Load(library, l => l.ContentTypes, l => l.Fields);
            tempCtx.ExecuteQueryRetry();

            if (ListItemFieldValues.FirstOrDefault(fv => fv.FieldType.StartsWith("TaxonomyField")) != null)
            {
                taxonomySession = TaxonomySession.GetTaxonomySession(tempCtx);
                termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            }

            var lookupFields = library.Fields.Where(f => f.FieldTypeKind == FieldType.Lookup && ListItemFieldValues.FirstOrDefault(fv => fv.FieldName == f.InternalName) != null);
            var lookupQueries = new Dictionary<string, string[]>();

            foreach (var field in lookupFields)
            {
                var lookupField = tempCtx.CastTo<FieldLookup>(field);
                lookupField.EnsureProperties(lf => lf.LookupList, lf => lf.LookupField);
                var lookupListId = lookupField.LookupList;
                var lookupList = tempCtx.Web.Lists.GetById(Guid.Parse(lookupListId));
                tempCtx.Load(lookupList, l => l.Title);
                try
                {
                    tempCtx.ExecuteQueryRetry();
                }
                catch
                {
                    //Ignore
                }

                lookupQueries[field.InternalName] = new string[]
                {
                    lookupList.Title,
                    $"<View><ViewFields><FieldRef Name='{lookupField.LookupField}'/></ViewFields><Query><Where><Contains><FieldRef Name='{lookupField.LookupField}' /><Value Type='Text'>#VALUE#</Value></Contains></Where></Query><RowLimit>1</RowLimit></View>"
                };
            }

            foreach (var fieldInfo in ListItemFieldValues)
            {
                if (!specialNames.Contains(fieldInfo.FieldName) &&
                    !specialTypes.Contains(fieldInfo.FieldType))
                {
                    item[fieldInfo.FieldName] =
                        fieldInfo.Value.Replace("{@WebUrl}", web.Url)
                            .Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl);
                }
                else if (fieldInfo.FieldType == "DateTime")
                {
                    item[fieldInfo.FieldName] = DateTime.ParseExact(fieldInfo.Value, "yyyy-MM-ddTHH:mm:ss.fffZ",
                        CultureInfo.InvariantCulture);
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
                        //ignore
                        //TODO: Notification event
                    }
                }
                else if (fieldInfo.FieldType == "Lookup")
                {
                    if (lookupQueries.ContainsKey(fieldInfo.FieldName))
                    {
                        var lookupListTitle = lookupQueries[fieldInfo.FieldName][0];
                        var viewXml = lookupQueries[fieldInfo.FieldName][1].Replace(
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
                                //TODO: Notify
                            }
                            else
                            {
                                item[fieldInfo.FieldName] = listItems[0].Id;
                            }
                        }
                        catch (Exception ex)
                        {
                            //TODO: Notify
                            //OnNotify(ProvisioningNotificationLevels.Verbose, $"Unable to find {fieldInfo.Value} in {lookupListTitle}. An exception occurred: {ex.Message}. Skipping attempt to set list item value.");
                        }
                    }
                }
                else if (fieldInfo.FieldType.StartsWith("TaxonomyField"))
                {
                    tempCtx.Web.EnsureProperty(w => w.Language);
                    var termSetName = fieldInfo.Value.GetInnerText("{@TermSet:", "}");
                    var termNames = fieldInfo.Value.GetInnerText("{@Terms:", "}").Split(';');
                    var termSets = termStore.GetTermSetsByName(termSetName, (int)tempCtx.Web.Language);
                    tempCtx.Load(termSets, ts => ts.Include(t => t.Id));
                    tempCtx.ExecuteQueryRetry();

                    if (termSets.Count == 0)
                    {
                        //OnNotify(ProvisioningNotificationLevels.Verbose,
                        //    $"Unable to find term set {termSetName}. Skipping list item field!");
                    }
                    else if (termNames.Length == 0)
                    {
                        //OnNotify(ProvisioningNotificationLevels.Verbose,
                        //    $"Bad field value token {fieldInfo.Value}. Skipping list item field!");
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
                                //OnNotify(ProvisioningNotificationLevels.Verbose, $"Unable to find term {termName}. Skipping list item field!");
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
                else if (fieldInfo.FieldName == "ContentType")
                {

                    var itemCType =
                        library.ContentTypes.FirstOrDefault(ctype => ctype.Name == fieldInfo.Value);
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
        }

        protected void AddListViewWebPart(ClientContext ctx, Web web, string resourceKey, string title, string zoneId, int order, string listName)
        {
            AddListViewWebPart(ctx, web, resourceKey, title, zoneId, order, listName, false, Assembly.GetCallingAssembly());
        }

        protected void AddListViewWebPart(ClientContext ctx, Web web, string resourceKey, string title, string zoneId, int order, string listName,
            bool isCalendar)
        {
            AddListViewWebPart(ctx, web, resourceKey, title, zoneId, order, listName, isCalendar, Assembly.GetExecutingAssembly());
        }

        protected void AddListViewWebPart(ClientContext ctx, Web web, string resourceKey, string title, string zoneId, int order, string listName,
            bool isCalendar, Assembly assembly)
        {
            var list = web.Lists.GetByTitle(listName);
            ctx.Load(web, w => w.ServerRelativeUrl);
            ctx.Load(list, l => l.Id, l => l.RootFolder.ServerRelativeUrl);
            ctx.ExecuteQueryRetry();

            var listNameForReplace = list.Id.ToString().ToUpper();
            var listId = list.Id.ToString().ToLower();
            var webPartXml = Encoding.UTF8.GetString(Utility.GetFile(resourceKey, false, assembly));

            //Substring(1) strips off the leading BOM from the file
            //TODO: find a better solution
            webPartXml =
                webPartXml.Replace("{ListName}", listNameForReplace)
                    .Replace("{ListId}", listId)
                    .Replace("{ListUrl}", list.RootFolder.ServerRelativeUrl)
                    .Substring(1);

            var limitedWebPartManager = File.GetLimitedWebPartManager(PersonalizationScope.Shared);
            var def = limitedWebPartManager.ImportWebPart(webPartXml);
            def.WebPart.Title = title;
            limitedWebPartManager.AddWebPart(def.WebPart, zoneId, order);

            if (!isCalendar)
            {
                GetNewView(ctx, list, title);
            }
        }

        public void AddWikiOrPublishingPageWebParts(ClientContext ctx, Web web, string contentFieldName)
        {
            if (WebParts == null || WebParts.Count == 0)
            {
                var pageContent = ListItemFieldValues.Find(p => p.FieldName == contentFieldName)?.Value ?? String.Empty;

                if (web.ServerRelativeUrl != "/")
                {
                    File.ListItemAllFields[contentFieldName] = pageContent.Replace("{@WebUrl}", web.Url).Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl);
                }
                else
                {
                    File.ListItemAllFields[contentFieldName] = pageContent.Replace("{@WebUrl}", web.Url).Replace("{@WebServerRelativeUrl}", "");
                }
                File.ListItemAllFields.Update();

                return;
            }

            var newIdMappings = new Dictionary<string, string>();

            var limitedWebPartManager = File.GetLimitedWebPartManager(PersonalizationScope.Shared);

            AddWikiOrPublishingContentPageWebParts(ctx, web, newIdMappings, limitedWebPartManager);
            UpdateWikiOrPublishingContentWithStorageKeys(newIdMappings, web, contentFieldName);
            LoadWebPartManagerFromSharePoint(ctx, limitedWebPartManager);
            MoveWebPartsToWikiOrPublishingContentEditorWebPartZone(ctx, newIdMappings, limitedWebPartManager);
            SetPageListViews(ctx, web, newIdMappings);
        }

        public void AddWebPartPageWebParts(ClientContext ctx, Web web)
        {
            var limitedWebPartManager = File.GetLimitedWebPartManager(PersonalizationScope.Shared);
            var newIdMappings = new Dictionary<string, string>();

            //Add from last to first in the zone to
            //avoid dealing with Sequence
            var orderedWebParts = WebPartPageZoneMappings.OrderByDescending(zm => zm.Position);

            foreach (var zoneMapping in orderedWebParts)
            {
                var wpXml = WebParts[zoneMapping.WebPartId].Replace("{@WebUrl}", web.Url).Replace("{@WebServerRelativeUrl}",
                    web.ServerRelativeUrl != "/" ? web.ServerRelativeUrl : "");

                //This is a crude way to go about cooercing the version numbers
                //for files created on SPO/SP2016 for SP2013 on prem
                //probably valid 99.999% of the time, but if it isn't for you, sorry!
                if (ctx.ServerLibraryVersion.Major == 15)
                {
                    wpXml = wpXml.Replace("16.0.0.0", "15.0.0.0");
                }

                var hasListIdToken = wpXml.Contains(@"{@ListId:");
                var hasListUrlToken = wpXml.Contains(@"{@ListUrl:");
                var listTitle = string.Empty;

                if (hasListIdToken || hasListUrlToken)
                {
                    listTitle = wpXml.GetInnerText("{@ListId:", "}");
                    wpXml = ReplaceListTokens(wpXml, ctx, web, false);
                }

                var def = limitedWebPartManager.ImportWebPart(wpXml);
                def = limitedWebPartManager.AddWebPart(def.WebPart, zoneMapping.ZoneId, 0);
                ctx.Load(def);
                ctx.Load(def.WebPart);
                ctx.ExecuteQueryRetry();

                newIdMappings[zoneMapping.WebPartId] = def.Id.ToString().Replace("{", "").Replace("}", "").ToLower();

                if (WebPartPageWebPartListViews.ContainsKey(zoneMapping.WebPartId))
                {
                    //Fallback code for lack of WebPartPages web service to app identities
                    //Need config to do one or other instead of failing over
                    GetNewView(ctx, zoneMapping.WebPartId, listTitle);
                }
            }
            SetPageListViews(ctx, web, newIdMappings);
        }

        private void SetPageListViews(ClientContext ctx, Web web, Dictionary<string, string> newIdMappings)
        {
            Dictionary<string, string> viewCollection;
            if (WikiPageWebPartListViews == null || WikiPageWebPartListViews.Count == 0)
                viewCollection = WebPartPageWebPartListViews;
            else viewCollection = WikiPageWebPartListViews;

            if (viewCollection == null || viewCollection.Count == 0) return;

            var shouldExecuteQuery = false;

            var soapFailed = false;

            foreach (var webPartId in WebParts.Keys)
            {
                var listTitle = WebParts[webPartId].GetInnerText("{@ListId:", "}", true);
                if (viewCollection.ContainsKey(webPartId))
                {
                    if (!soapFailed)
                    {
                        try
                        {
                            //Get the real view id from the web part from the page 
                            var partXml = WebPartUtility.GetWebPart(ctx, web, File.ServerRelativeUrl,
                                Guid.Parse(newIdMappings[webPartId]));
                            var viewId = partXml.GetInnerText("View Name=\"", "\"", true);
                            var list = web.Lists.GetByTitle(listTitle);
                            var view = list.Views.GetById(Guid.Parse(viewId));
                            view.ListViewXml = viewCollection[webPartId];
                            view.Update();
                            shouldExecuteQuery = true;
                        }
                        catch
                        {
                            soapFailed = true;
                        }
                    }

                    if (soapFailed)
                    {
                        var list = ctx.Web.Lists.GetByTitle(listTitle);
                        ctx.Load(list.Views,
                            v => v.Include(view => view.Id, view => view.Hidden).Where(view => view.Hidden));

                        ctx.ExecuteQueryRetry();

                        var viewCount = list.Views.Count;
                        var lastView = list.Views[viewCount - 1];
                        lastView.ListViewXml = viewCollection[webPartId];
                        lastView.Update();
                        shouldExecuteQuery = true;
                    }
                }
            }
            if (shouldExecuteQuery) ctx.ExecuteQueryRetry();
        }

        private static void MoveWebPartsToWikiOrPublishingContentEditorWebPartZone(ClientContext ctx,
            Dictionary<string, string> newIdMappings, LimitedWebPartManager limitedWebPartManager)
        {
            foreach (var wp in limitedWebPartManager.WebParts)
            {
                if (newIdMappings.Values.Contains(wp.Id.ToString().Replace("{", "").Replace("}", "").ToLower()))
                {
                    wp.MoveWebPartTo("wpz", 0);
                    wp.SaveWebPartChanges();
                    ctx.ExecuteQueryRetry();
                }
            }
        }

        private static void LoadWebPartManagerFromSharePoint(ClientContext ctx,
            LimitedWebPartManager limitedWebPartManager)
        {
            ctx.Load(limitedWebPartManager.WebParts, wps => wps.Include(wp => wp.Id));
            ctx.ExecuteQueryRetry();
        }

        private void UpdateWikiOrPublishingContentWithStorageKeys(Dictionary<string, string> newIdMappings, Web web, string contentFieldName)
        {
            var pageContent = ListItemFieldValues.Find(p => p.FieldName == contentFieldName)?.Value ?? String.Empty;

            File.ListItemAllFields[contentFieldName] =
                WikiPageUtility.GetUpdatedWikiContentText(pageContent, WikiPageWebPartStorageKeyMappings,
                    newIdMappings)
                    .Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl != "/" ? web.ServerRelativeUrl : "");
            File.ListItemAllFields.Update();
            File.Context.ExecuteQuery();
        }

        private void AddWikiOrPublishingContentPageWebParts(ClientContext ctx, Web web, Dictionary<string, string> newIdMappings,
            LimitedWebPartManager limitedWebPartManager)
        {
            foreach (var key in WebParts.Keys)
            {
                var wpXml = WebParts[key].Replace("{@WebUrl}", web.Url).Replace("{@WebServerRelativeUrl}",
                    web.ServerRelativeUrl != "/" ? web.ServerRelativeUrl : "");

                //This is a crude way to go about cooercing the version numbers
                //for files created on SPO/SP2016 for SP2013 on prem
                //probably valid 99.999% of the time, but if it isn't for you, sorry!
                if (ctx.ServerLibraryVersion.Major == 15)
                {
                    wpXml = wpXml.Replace("16.0.0.0", "15.0.0.0");
                }

                var hasListIdToken = wpXml.Contains(@"{@ListId:");
                var hasListUrlToken = wpXml.Contains(@"{@ListUrl:");

                if (hasListIdToken || hasListUrlToken)
                {
                    wpXml = ReplaceListTokens(wpXml, ctx, web, false);
                }

                var def = limitedWebPartManager.ImportWebPart(wpXml);
                def = limitedWebPartManager.AddWebPart(def.WebPart, "Bottom", 0);
                ctx.Load(def);
                ctx.Load(def.WebPart);
                ctx.ExecuteQueryRetry();

                newIdMappings[key] = def.Id.ToString().Replace("{", "").Replace("}", "").ToLower();
            }
        }

        //Original for backward compatability of older manifests pre-builder style
        protected void GetNewView(ClientContext ctx, List list, string title)
        {
            ctx.Load(list.Views,
                v => v.Include(view => view.Id, view => view.Hidden).Where(view => view.Hidden));

            ctx.ExecuteQueryRetry();

            var viewCount = list.Views.Count;
            var lastView = list.Views[viewCount - 1];

            if (!Views.ContainsKey(lastView.Id))
            {
                Views.Add(lastView.Id, lastView);
                if (!ViewMappings.ContainsKey(title))
                {
                    ViewMappings.Add(title, lastView.Id);
                }
            }
        }

        //Original for backward compatability of older manifests pre-builder style
        protected void UpdateViews()
        {
            if (ViewSchemas != null && ViewSchemas.Count > 0)
            {
                foreach (var key in ViewSchemas.Keys)
                {
                    if (ViewMappings.ContainsKey(key))
                    {
                        Views[ViewMappings[key]].ListViewXml = ViewSchemas[key];
                        Views[ViewMappings[key]].Update();
                    }
                }
            }
            if (ViewMappings2 != null && ViewMappings2.Count > 0)
                foreach (var viewMapping in ViewMappings2)
                {
                    if (WebPartPageWebPartListViews.ContainsKey(viewMapping.Key))
                    {
                        Views[viewMapping.Value].ListViewXml = WebPartPageWebPartListViews[viewMapping.Key];
                        Views[viewMapping.Value].Update();
                    }
                }
        }

        //New one for fallback if unable to use WebPartPages service
        private void GetNewView(ClientContext ctx, string webPartId, string listTitle)
        {
            var list = ctx.Web.Lists.GetByTitle(listTitle);
            ctx.Load(list.Views,
                v => v.Include(view => view.Id, view => view.Hidden).Where(view => view.Hidden));

            ctx.ExecuteQueryRetry();

            var viewCount = list.Views.Count;
            var lastView = list.Views[viewCount - 1];

            if (!Views.ContainsKey(lastView.Id))
            {
                Views.Add(lastView.Id, lastView);
                if (!ViewMappings2.ContainsKey(webPartId))
                {
                    ViewMappings2.Add(webPartId, lastView.Id);
                }
            }
        }
    }
}