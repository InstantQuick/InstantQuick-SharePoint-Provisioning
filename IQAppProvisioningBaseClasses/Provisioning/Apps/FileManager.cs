using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;
using static IQAppProvisioningBaseClasses.Constants;
using Microsoft.SharePoint.Client.Publishing;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class FileManager : ProvisioningManagerBase
    {
        public virtual List<string> Folders { get; set; }
        public virtual Dictionary<string, FileCreator> Creators { get; set; }

        public void ProvisionAll(ClientContext ctx, Web web, string baseFilePath, List<string> folders,
            Dictionary<string, FileCreator> creators)
        {
            Folders = folders;

            CreateFolders(ctx, web);
        }

        public void ProvisionAll(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            try
            {
                Folders = manifest.Folders;

                CreateFolders(ctx, web);
                Creators = manifest.FileCreators;

                switch (manifest.StorageType)
                {
                    case StorageTypes.AzureStorage:
                    case StorageTypes.FileSystem:
                        ProvisionFromStorage(ctx, web, manifest);
                        break;
                    case StorageTypes.Assembly:
                    default:
                        throw new InvalidOperationException("Storage type is invalid.");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error creating files " + web.Url + " | " + "manifest=" + manifest.ManifestName + " | " +
                                 ex);
                throw;
            }
        }

        public void ProvisionAll(ClientContext ctx, Web web, bool isHostWeb, Assembly assembly)
        {
            try
            {
                CreateFolders(ctx, web);

                if (Creators != null && Creators.Count > 0)
                {
                    foreach (var key in Creators.Keys)
                    {
                        try
                        {
                            var creator = Creators[key];
                            if (!FileExists(creator, ctx, creator.ForceOverwrite))
                            {
                                byte[] file;
                                if (!isHostWeb || string.IsNullOrEmpty(creator.HostWebResourceKey))
                                {
                                    file = Utility.GetFile(key, creator.IsBinary, assembly);
                                }
                                else
                                {
                                    file = Utility.GetFile(creator.HostWebResourceKey, creator.IsBinary, assembly);
                                }

                                file = creator.PrepareFile(file, ctx);
                                Creators[key].File = UploadFile(ctx, creator.List, file, creator.Url);
                                ctx.Load(Creators[key].File, f => f.ServerRelativeUrl);
                                creator.Created = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Error getting resource " + key + " | " + ex);
                            throw;
                        }
                    }
                    ctx.ExecuteQueryRetry();

                    if (isHostWeb) ApplySecurity(ctx);

                    foreach (var key in Creators.Keys)
                    {
                        Creators[key].SetProperties(ctx, web);
                    }
                    ctx.ExecuteQueryRetry();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error creating files " + web.Url + " | " + ex);
                throw;
            }
        }

        private void ProvisionFromStorage(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            List wikiPagesLibrary = null;
            List publishingPagesLibrary = null;
            if (Creators != null && Creators.Count > 0)
            {
                foreach (var fileCreator in Creators.Values)
                {
                    try
                    {
                        if (!FileExists(fileCreator, ctx, web, fileCreator.ForceOverwrite))
                        {
                            if ((fileCreator.ContentType != null && fileCreator.ContentType == "Wiki Page") || (fileCreator.ContentTypeId != null && fileCreator.ContentTypeId.StartsWith(WikiPageContentTypeId)))
                            {
                                OnNotify(ProvisioningNotificationLevels.Verbose, "Creating wiki page " + fileCreator.Url);
                                if (wikiPagesLibrary == null)
                                {
                                    wikiPagesLibrary = web.Lists.GetByTitle(fileCreator.List);
                                    ctx.Load(wikiPagesLibrary.RootFolder, f => f.ServerRelativeUrl);
                                    ctx.ExecuteQueryRetry();
                                }
                                var fileUrl = fileCreator.Url;
                                if (web.ServerRelativeUrl != "/") fileUrl = web.ServerRelativeUrl + fileUrl;

                                fileCreator.File = wikiPagesLibrary.RootFolder.Files.AddTemplateFile(fileUrl,
                                    TemplateFileType.WikiPage);
                                ctx.Load(fileCreator.File, f => f.ServerRelativeUrl);
                                ctx.ExecuteQueryRetry();
                                fileCreator.AddWikiOrPublishingPageWebParts(ctx, web, WikiPageContentFieldName);
                                fileCreator.Created = true;
                            }
                            else if (fileCreator.ContentTypeId != null && fileCreator.ContentTypeId.StartsWith(PublishingPageContentTypeId))
                            {
                                OnNotify(ProvisioningNotificationLevels.Verbose, "Creating publishing page " + fileCreator.Url);
                                if (publishingPagesLibrary == null)
                                {
                                    publishingPagesLibrary = web.Lists.GetByTitle(fileCreator.List);
                                    ctx.Load(publishingPagesLibrary.RootFolder, f => f.ServerRelativeUrl);
                                    ctx.ExecuteQueryRetry();
                                }
                                var fileUrl = fileCreator.Url;
                                if (web.ServerRelativeUrl != "/") fileUrl = web.ServerRelativeUrl + fileUrl;

                                var pageName = fileCreator.ListItemFieldValues.Find(p => p.FieldName == "FileLeafRef")?.Value;
                                var pageLayout = fileCreator.ListItemFieldValues.Find(p => p.FieldName == "PublishingPageLayout")?.Value;
                                

                                if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(pageLayout) || !pageLayout.Contains(", "))
                                {
                                    OnNotify(ProvisioningNotificationLevels.Verbose,
                                        $"Invalid publishing page data for {fileCreator.Url}! Skipping");
                                }
                                else
                                {
                                    try
                                    {
                                        pageLayout = pageLayout.Split(',')[0].Replace("{@WebUrl}", web.ServerRelativeUrl);
                                        var pageLayoutListItem =
                                            web.GetFileByServerRelativeUrl(pageLayout).ListItemAllFields;
                                        ctx.Load(pageLayoutListItem);
                                        PublishingPageInformation publishingPageInfo = new PublishingPageInformation()
                                        {
                                            Name = pageName,
                                            Folder = publishingPagesLibrary.RootFolder,
                                            PageLayoutListItem = pageLayoutListItem
                                        };
                                        PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(ctx, web);
                                        fileCreator.File = publishingWeb.AddPublishingPage(publishingPageInfo).ListItem.File;
                                        ctx.Load(fileCreator.File, f => f.ServerRelativeUrl);
                                        ctx.ExecuteQueryRetry();
                                        fileCreator.AddWikiOrPublishingPageWebParts(ctx, web, PublishingPageContentFieldName);
                                        fileCreator.Created = true;
                                    }
                                    catch
                                    {
                                        OnNotify(ProvisioningNotificationLevels.Verbose,
                                        $"Invalid publishing page data for {fileCreator.Url}! Unable to find page layout at {pageLayout}. If the page layout is in the manifest, ensure that it appears before the pages that depend on it. Skipping");
                                    }
                                }
                            }
                            else if ((fileCreator.ContentType != null && fileCreator.ContentType == "Web Part Page") || (fileCreator.ContentTypeId != null && fileCreator.ContentTypeId.StartsWith(BasicPageContentTypeId)))
                            {
                                OnNotify(ProvisioningNotificationLevels.Verbose,
                                    "Creating web part page " + fileCreator.Url);
                                var file = GetFileFromStorage(manifest, fileCreator);

                                file = fileCreator.PrepareFile(file, ctx, web, true);
                                fileCreator.File = UploadFile(ctx, web, fileCreator.List, file, fileCreator.Url);
                                ctx.Load(fileCreator.File, f => f.ServerRelativeUrl);
                                ctx.ExecuteQueryRetry();
                                fileCreator.AddWebPartPageWebParts(ctx, web);
                                fileCreator.Created = true;
                            }
                            else
                            {
                                OnNotify(ProvisioningNotificationLevels.Verbose, "Creating file " + fileCreator.Url);
                                var file = GetFileFromStorage(manifest, fileCreator);
                                if (!fileCreator.IsBinary)
                                    file = fileCreator.PrepareFile(file, ctx, web, fileCreator.ContentType != null && fileCreator.ContentType == "Workflow");
                                fileCreator.File = UploadFile(ctx, web, fileCreator.List, file, fileCreator.Url);
                                ctx.Load(fileCreator.File, f => f.ServerRelativeUrl);
                                fileCreator.Created = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnNotify(ProvisioningNotificationLevels.Normal,
                            "Error creating file " + fileCreator.RelativeFilePath + " | " + ex);
                        Trace.TraceError("Error creating file " + fileCreator.RelativeFilePath + " | " + ex);
                        throw;
                    }
                }

                if (!ctx.Web.IsPropertyAvailable("AppInstanceId"))
                {
                    ctx.Load(ctx.Web, w => w.AppInstanceId);
                    ctx.ExecuteQueryRetry();
                }
                if (ctx.Web.AppInstanceId == default(Guid)) ApplySecurity(ctx);

                foreach (var key in Creators.Keys)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Setting properties for " + Creators[key].Url);
                    Creators[key].SetProperties(ctx, web);
                }
                ctx.ExecuteQueryRetry();
            }
        }

        private byte[] GetFileFromStorage(AppManifestBase manifest, FileCreator fileCreator)
        {
            byte[] file;
            if (manifest.StorageType == StorageTypes.FileSystem)
            {
                file = Utility.GetFile(fileCreator.RelativeFilePath, fileCreator.IsBinary, manifest.BaseFilePath);
            }
            else
            {
                var pfc = fileCreator as ProvisioningFileCreator;
                if (pfc == null)
                {
                    var storageInfo = manifest.GetAzureStorageInfo();
                    var blobStorage = new BlobStorage(storageInfo.Account, storageInfo.AccountKey, storageInfo.Container);
                    file = blobStorage.DownloadToByteArray(fileCreator.Url);
                }
                else
                {
                    var storageInfo = pfc.GetAzureStorageInfo();
                    var blobStorage = new BlobStorage(storageInfo.Account, storageInfo.AccountKey, storageInfo.Container);
                    file = blobStorage.DownloadToByteArray(fileCreator.Url);
                }
            }
            return file;
        }

        private void ApplySecurity(ClientContext ctx)
        {
            if (ctx.Web.AppInstanceId != default(Guid)) return;

            var secureObjects = new List<SecureObjectCreator>();
            foreach (var creator in Creators.Values)
            {
                if (creator.SecurityConfiguration != null)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Applying security to " + creator.Url);
                    creator.SecurityConfiguration.SecurableObject = creator.File.ListItemAllFields;
                    creator.SecurityConfiguration.SecureObjectType = SecureObjectType.File;
                    secureObjects.Add(creator.SecurityConfiguration);
                }
            }
            if (secureObjects.Count > 0)
            {
                ctx.ExecuteQueryRetry();
                var secureObjectManager = new SecureObjectManager(ctx) { SecureObjects = secureObjects };
                secureObjectManager.ApplySecurity();
            }
        }

        public void CreateFolders(ClientContext ctx)
        {
            CreateFolders(ctx, ctx.Web);
        }

        /// <summary>
        ///     creates folders in the supplied web using the supplied client context
        /// </summary>
        /// <param name="ctx">the client context</param>
        /// <param name="web">the web in which to create the folders</param>
        public void CreateFolders(ClientContext ctx, Web web)
        {
            if (Folders != null)
            {
                foreach (var folder in Folders)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Ensuring folder " + folder);
                    AddButDontFail(folder, ctx, web);
                }
            }
        }

        /// <summary>
        ///     creates a folder in the supplied web using the supplied client context
        /// </summary>
        /// <param name="folder">the folder</param>
        /// <param name="ctx">the client context</param>
        /// <param name="web">the web</param>
        private void AddButDontFail(string folder, ClientContext ctx, Web web)
        {
            try
            {
                var root = web.RootFolder;
                if (folder.StartsWith("/")) folder = folder.Substring(1);
                root.Folders.Add(folder);
                ctx.ExecuteQueryRetry();
            }
            catch
            {
                // ignored
            }
        }

        public void DeleteAll(ClientContext ctx)
        {
            DeleteAll(ctx, ctx.Web);
        }

        /// <summary>
        ///     deletes all of the files and folders
        /// </summary>
        /// <param name="ctx">the client context</param>
        /// <param name="web">the web</param>
        public void DeleteAll(ClientContext ctx, Web web)
        {
            if (Creators != null)
            {
                foreach (var key in Creators.Keys)
                {
                    if (Creators[key].DeleteOnCleanup)
                    {
                        try
                        {
                            DeleteFile(web, Creators[key].List, Creators[key].Url);
                            ctx.ExecuteQueryRetry();
                        }
                        catch
                        {
                            Trace.WriteLine("Error deleting " + key);
                        }
                    }
                }
            }

            DeleteFolders(ctx, web);

            ctx.ExecuteQueryRetry();
        }

        public void DeleteFolders(ClientContext ctx)
        {
            DeleteFolders(ctx, ctx.Web);
        }

        /// <summary>
        ///     deletes the folders
        /// </summary>
        /// <param name="ctx">the client context</param>
        /// <param name="web">the web</param>
        public void DeleteFolders(ClientContext ctx, Web web)
        {
            if (Folders != null)
            {
                var rootFolders = web.RootFolder.Folders;
                ctx.Load(rootFolders);
                ctx.ExecuteQueryRetry();

                foreach (var f in Folders)
                {
                    var count = rootFolders.Count;
                    for (var i = count - 1; i >= 0; i--)
                    {
                        if (rootFolders[i].Name == f)
                        {
                            DeleteFolder(rootFolders[i], ctx);
                        }
                    }
                }
            }
        }

        private void DeleteFolder(Folder folder, ClientContext ctx)
        {
            ctx.Load(folder.Files);
            ctx.Load(folder.Folders);
            ctx.ExecuteQueryRetry();

            var count = folder.Files.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                folder.Files[i].DeleteObject();
            }

            count = folder.Folders.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                DeleteFolder(folder.Folders[i], ctx);
            }
            folder.DeleteObject();

            ctx.ExecuteQueryRetry();
        }

        public File UploadFile(ClientContext ctx, string list, byte[] file, string url)
        {
            return UploadFile(ctx, ctx.Web, list, file, url);
        }

        private File UploadFile(ClientContext ctx, Web web, string list, byte[] file, string url)
        {
            Folder folder;

            var fileCreationInformation = new FileCreationInformation
            {
                Content = file,
                Overwrite = true
            };
            if (web.ServerRelativeUrl != "/") url = web.ServerRelativeUrl + url;
            fileCreationInformation.Url = url;

            if (!string.IsNullOrEmpty(list))
            {
                var library = web.Lists.GetByTitle(list);
                folder = library.RootFolder;
            }
            else
            {
                folder = web.RootFolder;
            }

            var uploadFile = folder.Files.Add(fileCreationInformation);
            if (!string.IsNullOrEmpty(list))
            {
                ctx.Load(uploadFile, f => f.ListItemAllFields);
            }
            ctx.ExecuteQueryRetry();

            return uploadFile;
        }

        public bool FileExists(FileCreator creator, ClientContext ctx, bool deleteIfFound)
        {
            return FileExists(creator, ctx, ctx.Web, deleteIfFound);
        }

        private bool FileExists(FileCreator fileCreator, ClientContext ctx, Web web, bool deleteIfFound)
        {
            var exists = true;

            try
            {
                var url = fileCreator.Url;
                if (web.ServerRelativeUrl != "/") url = web.ServerRelativeUrl + url;
                var file = web.GetFileByServerRelativeUrl(url);
                ctx.Load(file, f => f.Exists);
                ctx.ExecuteQueryRetry();
                if (file.Exists)
                {
                    if (deleteIfFound)
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose,
                            "Deleting file for overwrite " + fileCreator.Url);
                        file.DeleteObject();
                        try
                        {
                            ctx.ExecuteQueryRetry();
                            exists = false;
                        }
                        catch(Exception ex)
                        {
                            OnNotify(ProvisioningNotificationLevels.Normal, $"Unable to delete {fileCreator.Url}. Error was {ex.Message}");
                        }
                    }
                    else
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose,
                            "File exists and not set to overwrite " + fileCreator.Url);
                        fileCreator.File = file;
                    }
                }
                else
                {
                    exists = false;
                }
            }
            catch
            {
                //Workaround for CSOM regression defect
                //wherein the check to see if the file exists throws an error if the file doesn't exist!
                exists = false;
            }

            return exists;
        }

        /// <summary>
        ///     deletes a file
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listTitle"></param>
        /// <param name="url"></param>
        private void DeleteFile(Web web, string listTitle, string url)
        {
            Folder folder;

            if (!string.IsNullOrEmpty(listTitle))
            {
                var list = web.Lists.GetByTitle(listTitle);
                folder = list.RootFolder;

                if (web.ServerRelativeUrl != "/")
                {
                    url = web.ServerRelativeUrl + url;
                }
            }
            else
            {
                folder = web.RootFolder;
            }

            var file = folder.Files.GetByUrl(url);
            file.DeleteObject();
        }
    }
}