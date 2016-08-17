using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using HtmlAgilityPack;
using IQAppProvisioningBaseClasses;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.WebParts;
using ScrapySharp.Extensions;
using File = Microsoft.SharePoint.Client.File;
using static IQAppProvisioningBaseClasses.Constants;

namespace IQAppManifestBuilders
{
    public class FoldersAndFileCreator
    {
        public virtual List<string> Folders { get; set; }
        public virtual Dictionary<string, FileCreator> FileCreators { get; set; }
    }

    public class FileCreatorBuilder : CreatorBuilderBase
    {
        public NetworkCredential RequestContextCredentials { get; set; }

        public AppManifestBase GetFileCreator(ClientContext ctx, Web web, string fileWebRelativeUrl,
            string downloadFolderPath)
        {
            return GetFileCreator(ctx, web, fileWebRelativeUrl, downloadFolderPath, string.Empty, false);
        }

        public AppManifestBase GetFileCreator(ClientContext ctx, Web web, string fileWebRelativeUrl,
            string downloadFolderPath, string appManifestJson, bool getRelatedFileCreators)
        {
            var js = new JavaScriptSerializer();

            var existingManifest = js.Deserialize<AppManifestBase>(appManifestJson);
            
            return GetFileCreator(ctx, web, fileWebRelativeUrl, downloadFolderPath, existingManifest,
                getRelatedFileCreators);
        }

        public AppManifestBase GetFileCreator(ClientContext ctx, Web web, string fileWebRelativeUrl,
            string downloadFolderPath, AppManifestBase existingManifest, bool getRelatedFileCreators)
        {
            if (!fileWebRelativeUrl.StartsWith("/")) fileWebRelativeUrl = "/" + fileWebRelativeUrl;
            fileWebRelativeUrl = fileWebRelativeUrl.Replace("%20", " ");
            OnVerboseNotify("Getting file creator for " + fileWebRelativeUrl);

            if (string.IsNullOrEmpty(downloadFolderPath))
            {
                downloadFolderPath = existingManifest.BaseFilePath;
            }

            existingManifest = existingManifest ?? new AppManifestBase();
            existingManifest.FileCreators = existingManifest.FileCreators ?? new Dictionary<string, FileCreator>();
            existingManifest.Folders = existingManifest.Folders ?? new List<string>();

            var fileCreator = GetFoldersAndFileCreatorFromSite(ctx, web, fileWebRelativeUrl, downloadFolderPath,
                getRelatedFileCreators, existingManifest);
            if (fileCreator == null)
            {
                return existingManifest;
            }
            if (fileCreator.FileCreators.ContainsKey(fileWebRelativeUrl))
            {
                existingManifest.FileCreators[fileWebRelativeUrl] = fileCreator.FileCreators[fileWebRelativeUrl];
                foreach (var folder in fileCreator.Folders)
                {
                    if (!existingManifest.Folders.Contains(folder)) existingManifest.Folders.Add(folder);
                }
                existingManifest.Folders.Sort();
            }

            return existingManifest;
        }

        public AppManifestBase RemoveFileCreator(ClientContext ctx, Web web, string fileWebRelativeUrl,
            AppManifestBase existingManifest)
        {
            if (!fileWebRelativeUrl.StartsWith("/")) fileWebRelativeUrl = "/" + fileWebRelativeUrl;
            fileWebRelativeUrl = fileWebRelativeUrl.Replace("%20", " ");

            if (existingManifest?.FileCreators == null || !existingManifest.FileCreators.ContainsKey(fileWebRelativeUrl))
                return existingManifest;

            existingManifest.FileCreators.Remove(fileWebRelativeUrl);

            if (existingManifest.StorageType == StorageTypes.AzureStorage)
            {
                var azureStorageInfo = existingManifest.GetAzureStorageInfo();
                var blobStorage = new BlobStorage(azureStorageInfo.Account, azureStorageInfo.AccountKey,
                    azureStorageInfo.Container);
                blobStorage.DeleteBlob(fileWebRelativeUrl);
            }
            //TODO: File system deletion

            return existingManifest;
        }

        private FoldersAndFileCreator GetFoldersAndFileCreatorFromSite(ClientContext ctx, Web web,
            string fileWebRelativeUrl, string downloadFolderPath, bool getRelatedFileCreators,
            AppManifestBase appManifest)
        {
            OnVerboseNotify("Getting file from site " + fileWebRelativeUrl);

            var retVal = new FoldersAndFileCreator
            {
                Folders = new List<string>(),
                FileCreators = new Dictionary<string, FileCreator>()
            };
            if (!fileWebRelativeUrl.StartsWith("/")) fileWebRelativeUrl = "/" + fileWebRelativeUrl;

            var parts = fileWebRelativeUrl.Split('/');
            var lastFolder = string.Empty;
            for (var i = 1; i < parts.Length - 1; i++)
            {
                lastFolder = lastFolder + "/" + parts[i];
                retVal.Folders.Add(lastFolder);
            }

            ctx.Load(web, w => w.ServerRelativeUrl);
            ctx.ExecuteQueryRetry();

            var fileUrl = fileWebRelativeUrl;
            if (web.ServerRelativeUrl != "/") fileUrl = web.ServerRelativeUrl + fileWebRelativeUrl;

            //Get the file
            var file = web.GetFileByServerRelativeUrl(fileUrl);
            ctx.Load(file, f => f.Exists);
            ctx.ExecuteQueryRetry();

            if (file.Exists)
            {
                OnVerboseNotify("Got file from site " + fileWebRelativeUrl);

                //Try to load the list items, if the list doesn't exist, ie. it's a folder there will be an exception
                ctx.Load(file, f => f.ListItemAllFields);
                ctx.Load(file.ListItemAllFields, l => l.ParentList, l => l.HasUniqueRoleAssignments,
                    l => l.FieldValuesForEdit, l => l.ContentType);

                var couldGetListItem = false;
                try
                {
                    ctx.ExecuteQueryRetry();
                    couldGetListItem = file.ListItemAllFields.ServerObjectIsNull != null &&
                                       !(bool)file.ListItemAllFields.ServerObjectIsNull;
                }
                catch
                {
                    // ignored
                }

                var fileName = parts[parts.Length - 1];

                retVal.FileCreators.Add(fileWebRelativeUrl, new FileCreator
                {
                    Name = fileName,
                    ForceOverwrite = true,
                    Url = fileWebRelativeUrl
                });

                var newFileCreator = retVal.FileCreators[fileWebRelativeUrl];

                newFileCreator.RelativeFilePath = fileWebRelativeUrl.Replace(@"/", @"\");

                if (couldGetListItem && file.ListItemAllFields.FieldValues.Count > 0)
                {
                    newFileCreator.List = file.ListItemAllFields.ParentList.Title;
                    AnalyzeSecurityConfiguration(newFileCreator, file.ListItemAllFields, ctx);

                    var listItemCreatorBuilder = new ListItemCreatorBuilder(ctx, web, file.ListItemAllFields.ParentList);

                    newFileCreator.ListItemFieldValues = listItemCreatorBuilder.GetListItemCreator(file.ListItemAllFields).FieldValues;

                    //Some item types have a list value, but a server null content type, such as dwp files
                    if (file.ListItemAllFields.ContentType.ServerObjectIsNull != true)
                    {
                        newFileCreator.ContentType = file.ListItemAllFields.ContentType.Name;
                        newFileCreator.ContentTypeId = file.ListItemAllFields.ContentType.StringId;

                        if (newFileCreator.ContentTypeId.StartsWith(WikiPageContentTypeId) || newFileCreator.ContentTypeId.StartsWith(PublishingPageContentTypeId))
                        {
                            var contentFieldName = newFileCreator.ContentTypeId.StartsWith(WikiPageContentTypeId) ? WikiPageContentFieldName : PublishingPageContentFieldName;
                            ProcessWikiOrPublishingPage(ctx, web, fileUrl, file, contentFieldName, newFileCreator);

                            if (getRelatedFileCreators)
                            {
                                var contentField = file.ListItemAllFields[contentFieldName]?.ToString() ?? string.Empty;
                                GetRelatedFileCreators(ctx, web, appManifest, downloadFolderPath, contentField);
                            }

                            newFileCreator.IsHomePage = IsHomePage(ctx, fileUrl);

                            //Wiki pages don't get downloaded, they are created from a template
                            return retVal;
                        }
                        //0x010107 is declarative workflow document
                        if (file.ListItemAllFields.ContentType.Id.StringValue.StartsWith(DeclarativeWorkflowDocumentContentTypeId))
                        {
                            DownloadWorkflowFile(ctx, web, downloadFolderPath, fileUrl, newFileCreator, appManifest);
                            newFileCreator.ContentType = "Workflow";
                            return retVal;
                        }
                    }
                }
                if (fileName.ToLowerInvariant().EndsWith(".aspx"))
                {
                    var listIds = GetWebListIds(ctx, web);
                    ProcessWebPartPage(ctx, web, fileUrl, file, newFileCreator, listIds);
                    DownloadAspxFile(ctx, downloadFolderPath, fileUrl, newFileCreator, listIds, appManifest);
                    if (newFileCreator.WebParts != null && newFileCreator.WebParts.Count > 0)
                    {
                        newFileCreator.ContentType = "Web Part Page";
                    }
                    newFileCreator.IsHomePage = IsHomePage(ctx, newFileCreator.Url);
                }
                else
                {
                    var listIds = GetWebListIds(ctx, web);
                    DownloadFile(ctx, downloadFolderPath, fileUrl, newFileCreator, listIds, appManifest);
                }
            }

            return retVal;
        }

        private bool IsHomePage(ClientContext ctx, string fileUrl)
        {
            var rootFolder = ctx.Web.RootFolder;
            ctx.Load(rootFolder, f => f.WelcomePage);
            ctx.ExecuteQueryRetry();
            return !string.IsNullOrEmpty(rootFolder.WelcomePage.ToLower()) &&
                   (rootFolder.WelcomePage.ToLower().EndsWith(fileUrl.ToLower()) ||
                    fileUrl.ToLower().EndsWith(rootFolder.WelcomePage.ToLower()));
        }

        private void GetRelatedFileCreators(ClientContext ctx, Web web, AppManifestBase appManifest,
            string downloadFolderPath, string pageContent)
        {
            OnVerboseNotify("Looking for related files from page content");
            var pageContentHtmlDoc = new HtmlDocument();
            pageContentHtmlDoc.LoadHtml(pageContent);

            var anchors = pageContentHtmlDoc.DocumentNode.CssSelect("a");
            var images = pageContentHtmlDoc.DocumentNode.CssSelect("img");

            foreach (var anchor in anchors)
            {
                var url = anchor.Attributes["href"].Value;
                if (web.ServerRelativeUrl != "/") url = url.Replace(web.ServerRelativeUrl, "");
                ProcessRelatedFile(ctx, web, appManifest, downloadFolderPath, url);
            }
            foreach (var img in images)
            {
                var url = img.Attributes["src"].Value;
                if (web.ServerRelativeUrl != "/") url = url.Replace(web.ServerRelativeUrl, "");
                ProcessRelatedFile(ctx, web, appManifest, downloadFolderPath, url);
            }
        }

        private void ProcessRelatedFile(ClientContext ctx, Web web, AppManifestBase appManifest,
            string downloadFolderPath, string url)
        {
            OnVerboseNotify("Processing related file " + url);
            var fileName = GetFileName(url);
            if (appManifest.FileCreators.ContainsKey(fileName)) return;

            GetFileCreator(ctx, web, url, downloadFolderPath, appManifest, true);
        }

        private string GetFileName(string url)
        {
            var parts = url.Split('/');
            //Strip the space encoding to avoid duplicates
            return parts[parts.Length - 1].Replace("%20", " ");
        }

        private void DownloadFile(ClientContext ctx, string downloadFolderPath, string fileUrl,
            FileCreator newFileCreator, Dictionary<Guid, List> listIds, AppManifestBase appManifest)
        {
            byte[] fileArray;
            string fileString;
            GetFile(ctx, fileUrl, out fileArray, out fileString);

            //As with all character encoding in .Net this is dicey. Assumption is a null byte indicates a binary file
            if (fileString.Contains("\0")) newFileCreator.IsBinary = true;
            else
            {
                fileString = TokenizeText(ctx.Web, listIds, fileString);
                fileArray = fileString.ToByteArrayUtf8();
            }

            PutFile(downloadFolderPath, newFileCreator, appManifest, fileArray);
        }

        private void SaveFileToAzure(string blobUrl, byte[] fileArray, AzureStorageInfo azureStorageInfo)
        {
            var blobStorage = new BlobStorage(azureStorageInfo.Account, azureStorageInfo.AccountKey,
                azureStorageInfo.Container);
            blobStorage.UploadFromByteArray(fileArray, blobUrl);
        }

        private void DownloadAspxFile(ClientContext ctx, string downloadFolderPath, string fileUrl,
            FileCreator newFileCreator, Dictionary<Guid, List> listIds, AppManifestBase appManifest)
        {
            byte[] fileArray;
            string fileString;
            GetFile(ctx, fileUrl, out fileArray, out fileString);

            //As with all character encoding in .Net this is dicey. Assumption is a null byte indicates a binary file
            if (fileString.Contains("\0")) newFileCreator.IsBinary = true;
            else
            {
                fileString = TokenizeText(ctx.Web, listIds, fileString);
                fileArray = fileString.ToByteArrayUtf8();
            }

            PutFile(downloadFolderPath, newFileCreator, appManifest, fileArray);
        }

        private void DownloadWorkflowFile(ClientContext ctx, Web web, string downloadFolderPath, string fileUrl,
            FileCreator newFileCreator, AppManifestBase appManifest)
        {
            byte[] fileArray;
            string fileString;
            GetFile(ctx, fileUrl, out fileArray, out fileString);

            fileArray = ReplaceWorkflowWebSpecificIDsWithTokens(ctx, web, fileString).ToByteArrayUtf8();

            //As with all character encoding in .Net this is dicey. Assumption is a null byte indicates a binary file
            if (fileString.Contains("\0")) newFileCreator.IsBinary = true;

            PutFile(downloadFolderPath, newFileCreator, appManifest, fileArray);

            //Store workflow for association on target site
            if (fileUrl.ToLowerInvariant().EndsWith(".wfconfig.xml"))
            {
                var webRelativeUrl = newFileCreator.Url;
                if (webRelativeUrl.StartsWith("/")) webRelativeUrl = webRelativeUrl.Substring(1);
                var def = new ClassicWorkflowCreator
                {
                    AssociateWorkflowMarkupConfigUrl = webRelativeUrl,
                    ConfigVersion = "V1.0"
                };
                //Althoguh the 2013 XOML version is V2.0, using it will fail!
                if (appManifest.ClassicWorkflowCreators == null)
                    appManifest.ClassicWorkflowCreators = new Dictionary<string, ClassicWorkflowCreator>();
                appManifest.ClassicWorkflowCreators[webRelativeUrl] = def;
            }
        }

        private void PutFile(string downloadFolderPath, FileCreator newFileCreator, AppManifestBase appManifest,
            byte[] fileArray)
        {
            if (appManifest.StorageType == StorageTypes.FileSystem)
            {
                var filePath = newFileCreator.RelativeFilePath;
                if (!downloadFolderPath.EndsWith(@"\")) downloadFolderPath = downloadFolderPath + @"\";

                SaveToFileSystem(downloadFolderPath, filePath, fileArray);
            }
            else if (appManifest.StorageType == StorageTypes.AzureStorage && appManifest.GetAzureStorageInfo() != null)
            {
                SaveFileToAzure(newFileCreator.Url, fileArray, appManifest.GetAzureStorageInfo());
            }
        }

        private void GetFile(ClientContext ctx, string fileUrl, out byte[] fileArray, out string fileString)
        {
            OnVerboseNotify("Downloading " + fileUrl);
            var fileInfo = File.OpenBinaryDirect(ctx, fileUrl);
            ctx.ExecuteQueryRetry();

            var dowloadFileStream = fileInfo.Stream;

            fileArray = GetFileAsByteArray(dowloadFileStream);
            fileString = GetFileString(fileArray);
        }

        private byte[] GetFileAsByteArray(Stream dowloadFileStream)
        {
            byte[] fileArray;
            using (var streamReader = new MemoryStream())
            {
                dowloadFileStream.CopyTo(streamReader);
                fileArray = streamReader.ToArray();
            }
            return fileArray;
        }

        private string GetFileString(byte[] fileArray)
        {
            return Encoding.UTF8.GetString(fileArray);
        }

        /// <summary>
        /// Gets the content and parts for wiki and publishing pages
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="web"></param>
        /// <param name="fileUrl"></param>
        /// <param name="file"></param>
        /// <param name="pageContentFieldName"></param>
        /// <param name="newFileCreator"></param>
        private void ProcessWikiOrPublishingPage(ClientContext ctx, Web web, string fileUrl, File file, string pageContentFieldName, FileCreator newFileCreator)
        {
            OnVerboseNotify("Processing Wiki or Publishing Page " + fileUrl);
            //Get the web part manager
            var webPartManager = TryToGetWebPartManager(ctx, file);
            if (webPartManager != null)
            {
                OnVerboseNotify("Processing web parts");
                var listIds = GetWebListIds(ctx, web);

                //Get each web part and do token replacements
                var webPartsXml = ReplaceWebPartWebSpecificIDsWithTokens(ctx, web, fileUrl, webPartManager, listIds);

                var pageContentListViews = GetListViewWebPartViewSchemas(ctx, web, webPartsXml);

                //Get the field contents
                var pageContent = file.ListItemAllFields.FieldValuesForEdit[pageContentFieldName];

                newFileCreator.WebParts = webPartsXml;
                newFileCreator.WikiPageWebPartListViews = pageContentListViews;

                //Extract the storage keys
                var storageKeys = WikiPageUtility.GetStorageKeysFromWikiContent(pageContent);

                //Fetch the page
                var page = RequestContextCredentials == null
                    ? WebPartUtility.GetWebPartPage(ctx, web, fileUrl)
                    : WebPartUtility.GetWebPartPage(web, RequestContextCredentials, fileUrl);

                //Search throught the page looking for the web part ID's that match the storage keys
                newFileCreator.WikiPageWebPartStorageKeyMappings = WikiPageUtility.GetStorageKeyMappings(page,
                    storageKeys);
            }
        }

        private void ProcessWebPartPage(ClientContext ctx, Web web, string fileUrl, File file,
            FileCreator newFileCreator, Dictionary<Guid, List> listIds)
        {
            OnVerboseNotify("Processing Web Part Page " + fileUrl);
            //Get the web part manager
            var webPartManager = TryToGetWebPartManager(ctx, file);
            if (webPartManager != null)
            {
                OnVerboseNotify("Processing web parts");


                //Get each web part and do token replacements
                var webPartsXml = ReplaceWebPartWebSpecificIDsWithTokens(ctx, web, fileUrl, webPartManager, listIds);

                var webPartListViews = GetListViewWebPartViewSchemas(ctx, web, webPartsXml);

                newFileCreator.WebParts = webPartsXml;
                newFileCreator.WebPartPageWebPartListViews = webPartListViews;

                var page = RequestContextCredentials == null
                    ? WebPartUtility.GetWebPartPage(ctx, web, fileUrl)
                    : WebPartUtility.GetWebPartPage(web, RequestContextCredentials, fileUrl);

                newFileCreator.WebPartPageZoneMappings = WebPartPageUtility.GetWebPartZoneMappings(page, webPartsXml);
            }
        }

        private Dictionary<string, string> GetListViewWebPartViewSchemas(ClientContext ctx, Web web,
            Dictionary<string, string> webPartsXml)
        {
            OnVerboseNotify("Getting view schemas for list view web parts");
            var retVal = new Dictionary<string, string>();
            foreach (var key in webPartsXml.Keys)
            {
                var xml = webPartsXml[key].Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl);
                if (xml.Contains("{@ListId:") && xml.Contains("View Name=\""))
                {
                    var listTitle = xml.GetInnerText("{@ListId:", "}", true);
                    if (!string.IsNullOrEmpty(listTitle))
                    {
                        var viewId = xml.GetInnerText("View Name=\"", "\"", true);
                        if (!string.IsNullOrEmpty(listTitle))
                        {
                            var listViewXml = GetListViewXml(ctx, web, listTitle, viewId);
                            if (!string.IsNullOrEmpty(listViewXml))
                            {
                                retVal[key] = listViewXml;
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        private string GetListViewXml(ClientContext ctx, Web web, string listTitle, string viewId)
        {
            var retVal = string.Empty;

            try
            {
                var list = web.Lists.GetByTitle(listTitle);
                var view = list.Views.GetById(Guid.Parse(viewId));
                ctx.Load(view, v => v.ListViewXml);
                ctx.ExecuteQueryRetry();

                XDocument document;
                using (var s = new StringReader(view.ListViewXml))
                {
                    document = XDocument.Load(s);
                }

                var element = document.Root;

                if (element != null)
                {
                    var reader = element.CreateReader();
                    reader.MoveToContent();
                    retVal = reader.ReadInnerXml();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to get view schema for " + listTitle + " | " + ex);
            }
            return retVal;
        }

        private Dictionary<string, string> ReplaceWebPartWebSpecificIDsWithTokens(ClientContext ctx, Web web,
            string fileUrl, LimitedWebPartManager webPartManager, Dictionary<Guid, List> listIds)
        {
            var webPartsXml = new Dictionary<string, string>();
            foreach (var wpmWebPart in webPartManager.WebParts)
            {
                var part = RequestContextCredentials == null
                    ? WebPartUtility.GetWebPart(ctx, web, fileUrl, wpmWebPart.Id)
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim()
                        .Replace("  ", " ")
                        .Replace("  ", " ")
                    : WebPartUtility.GetWebPart(web, RequestContextCredentials, fileUrl, wpmWebPart.Id)
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim()
                        .Replace("  ", " ")
                        .Replace("  ", " ");

                part = TokenizeText(web, listIds, part);
                webPartsXml[wpmWebPart.Id.ToString().ToLower().Replace("{", "").Replace("}", "")] = part;
            }
            return webPartsXml;
        }

        private string TokenizeText(Web web, Dictionary<Guid, List> listIds, string text)
        {
            foreach (var list in listIds)
            {
                var match = list.Key.ToString().Replace("{", "").Replace("}", "").ToLower();
                text = text.Replace(match, "{@ListId:" + list.Value.Title + "}")
                    .Replace(match.ToUpper(), "{@ListId:" + list.Value.Title + "}");
                text = text.Replace(list.Value.RootFolder.ServerRelativeUrl, "{@ListUrl:" + list.Value.Title + "}");
            }
            
            return TokenizeText(web, text);
        }

        private string TokenizeText(Web web, string text)
        {
            text = text.Replace(web.Url, "{@WebUrl}");
            if (web.ServerRelativeUrl != "/") text = text.Replace(web.ServerRelativeUrl, "{@WebServerRelativeUrl}");
            return text;
        }

        private string ReplaceWorkflowWebSpecificIDsWithTokens(ClientContext ctx, Web web, string fileString)
        {
            var listIds = GetWebListIds(ctx, web);
            var preparedFile = fileString;

            foreach (var list in listIds)
            {
                var match = list.Key.ToString().Replace("{", "").Replace("}", "").ToLower();
                if (preparedFile.Contains(match) || preparedFile.Contains(match.ToUpper()))
                {
                    preparedFile =
                        preparedFile.Replace(match, "{@ListId:" + list.Value.Title + "}")
                            .Replace(match.ToUpper(), "{@ListId:" + list.Value.Title + "}");
                    preparedFile = TokenizeListContentTypes(ctx, list.Value, preparedFile);
                }
            }

            if (web.ServerRelativeUrl != "/")
                preparedFile = preparedFile.Replace(web.ServerRelativeUrl, "{@WebServerRelativeUrl}");

            return preparedFile;
        }

        private string TokenizeListContentTypes(ClientContext ctx, List list, string preparedFile)
        {
            var listCTypes = list.ContentTypes;
            ctx.Load(listCTypes, cts => cts.Include(ct => ct.Id, ct => ct.Name));
            ctx.ExecuteQueryRetry();

            foreach (var cType in listCTypes)
            {
                preparedFile = preparedFile.Replace(cType.Id.ToString(),
                    $"{{@ListContentType: {list.Title}|{cType.Name}}}");
            }

            return preparedFile;
        }

        private LimitedWebPartManager TryToGetWebPartManager(ClientContext ctx, File file)
        {
            try
            {
                var webPartManager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
                ctx.Load(webPartManager.WebParts, wp => wp.Include(p => p.Id, p => p.WebPart));
                ctx.ExecuteQueryRetry();

                return webPartManager;
            }
            catch
            {
                return null;
            }
        }

        private void SaveToFileSystem(string downloadFolderPath, string filePath, byte[] fileArray)
        {
            if (!string.IsNullOrEmpty(downloadFolderPath) && !string.IsNullOrEmpty(filePath))
            {
                OnVerboseNotify("Downloading " + filePath);
                var outputPath = downloadFolderPath + @"\" + filePath;
                new FileInfo(outputPath).Directory?.Create();
                System.IO.File.WriteAllBytes(outputPath, fileArray);
            }
        }

        private void AnalyzeSecurityConfiguration(FileCreator creator, ListItem file, ClientContext ctx)
        {
            OnVerboseNotify("Analyzing security config");
            if (file.HasUniqueRoleAssignments)
            {
                ctx.Load(file, l => l.RoleAssignments);
                ctx.Load(file.RoleAssignments, ras => ras.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings));
                ctx.Load(file.ParentList, l => l.RoleAssignments);
                ctx.Load(file.ParentList.RoleAssignments,
                    ras => ras.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings));
                ctx.ExecuteQueryRetry();

                if (creator.SecurityConfiguration == null)
                {
                    creator.SecurityConfiguration = new SecureObjectCreator
                    {
                        SecureObjectType = SecureObjectType.File,
                        GroupRoleDefinitions = new Dictionary<string, string>()
                    };
                }

                creator.SecurityConfiguration.Title = creator.Name;
                creator.SecurityConfiguration.Url = creator.Url;
                creator.SecurityConfiguration.BreakInheritance = true;

                //First loop thorugh and see if the parent has principals with assignments not found in the file
                CheckShouldCopyExistingPermissions(creator, file);

                //Next loop through the assignments on the file and build the output
                FillFileGroupRoleDefinitions(creator, file, ctx);
            }
        }

        private void CheckShouldCopyExistingPermissions(FileCreator creator, ListItem file)
        {
            creator.SecurityConfiguration.CopyExisting = true;
            foreach (var roleAssignment in file.ParentList.RoleAssignments)
            {
                var principal = roleAssignment.Member;
                if (principal.PrincipalType == PrincipalType.SharePointGroup)
                {
                    var foundMatch = false;
                    foreach (var fileRoleAssignment in file.RoleAssignments)
                    {
                        if (fileRoleAssignment.Member.Id == roleAssignment.Member.Id)
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                    //The first unique ancestor has one that isn't in the file, assume break inheritance
                    if (!foundMatch)
                    {
                        creator.SecurityConfiguration.CopyExisting = false;
                        break;
                    }
                }
            }
        }

        private void FillFileGroupRoleDefinitions(FileCreator creator, ListItem file, ClientContext ctx)
        {
            ctx.Load(ctx.Web.AssociatedMemberGroup, g => g.Id);
            ctx.Load(ctx.Web.AssociatedOwnerGroup, g => g.Id);
            ctx.Load(ctx.Web.AssociatedVisitorGroup, g => g.Id);
            ctx.ExecuteQueryRetry();

            foreach (var roleAssignment in file.RoleAssignments)
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

                Debug.WriteLine(principalName);
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
                                foreach (var parentWebRoleAssignment in file.ParentList.RoleAssignments)
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

        private Dictionary<Guid, List> GetWebListIds(ClientContext ctx, Web web)
        {
            OnVerboseNotify("Getting web list ids " + web.ServerRelativeUrl);
            var retVal = new Dictionary<Guid, List>();
            ctx.Load(web.Lists,
                list =>
                    list.Include(property => property.Id, property => property.Title, property => property.RootFolder));
            ctx.ExecuteQueryRetry();
            foreach (var list in web.Lists)
            {
                retVal[list.Id] = list;
            }
            return retVal;
        }
    }
}