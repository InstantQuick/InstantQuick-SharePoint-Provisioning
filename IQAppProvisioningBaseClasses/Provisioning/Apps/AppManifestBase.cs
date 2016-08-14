using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using IQAppRuntimeResources;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public enum StorageTypes
    {
        Assembly,
        FileSystem,
        AzureStorage
    }

    public class AzureStorageInfo
    {
        public string Account { get; set; }
        public string AccountKey { get; set; }
        public string Container { get; set; }
    }

    public class AppManifestBase
    {
        //Not a property to avoid JSON serialization
        internal Assembly Assembly;

        internal AzureStorageInfo StorageInfo;
        //End not a property to avoid JSON serialization

        public virtual ResourceAssemblyInfo DefaultResourceAssembly { get; set; }
        public virtual List<ResourceAssemblyInfo> AdditionalResourceAssemblies { get; set; }
        public virtual string ProvisioningAssemblyName { get; set; }
        public virtual string ProvisioningClass { get; set; }
        public virtual Dictionary<string, string> Settings { get; set; }
        public virtual string ManifestId { get; set; } = Guid.NewGuid().ToString();
        public virtual string ManifestName { get; set; }
        public virtual string Version { get; set; } = "0.0.0.0";
        public virtual string RemoteHost { get; set; }
        public virtual StorageTypes StorageType { get; set; } = StorageTypes.FileSystem;

        /// <summary>
        ///     Represents base file path from which file locations are derived
        ///     This value is determined at runtime when provisioning the manifest from the context of the operation
        ///     Any persisted value is ignored
        /// </summary>
        public virtual string BaseFilePath { get; set; }

        public virtual Dictionary<string, GroupCreator> GroupCreators { get; set; }
        public virtual Dictionary<string, RoleDefinitionCreator> RoleDefinitions { get; set; }
        public virtual Dictionary<string, string> Fields { get; set; }
        public virtual Dictionary<string, ContentTypeCreator> ContentTypeCreators { get; set; }
        public virtual Dictionary<string, ListCreator> ListCreators { get; set; }
        public virtual List<string> Folders { get; set; }
        public virtual Dictionary<string, FileCreator> FileCreators { get; set; }
        public virtual Dictionary<string, ClassicWorkflowCreator> ClassicWorkflowCreators { get; set; }
        public virtual Dictionary<string, CustomActionCreator> CustomActionCreators { get; set; }
        public virtual Dictionary<string, FeatureAdderCreator> AddFeatures { get; set; }
        public virtual Dictionary<string, FeatureRemoverCreator> RemoveFeatures { get; set; }
        public virtual NavigationCreator Navigation { get; set; }
        public virtual LookAndFeelCreator LookAndFeel { get; set; }
        public virtual List<RemoteEventRegistrationCreator> RemoteEventRegistrationCreators { get; set; }
        public virtual string Description { get; set; }
        //This collection is used by apps with a host service resources manager, e.g. rserve.ashx 
        public virtual Dictionary<string, DataPageResource> RuntimeResources { get; set; }
        //This collection is used by pure client apps which use a local JS file for resource mappings
        public virtual Dictionary<string, ClientPageRuntimeResources> ClientPageRuntimeResources { get; set; }
        public virtual Dictionary<string, PageResource> ClientGlobalRuntimeResources { get; set; }

        public AppManifestBase Clone()
        {
            var js = new JavaScriptSerializer();
            var s = js.Serialize(this);
            return js.Deserialize<AppManifestBase>(s);
        }

        public virtual void SetAssembly(Assembly assembly)
        {
            Assembly = assembly;
        }

        public virtual Assembly GetAssembly()
        {
            return Assembly;
        }

        public void SetAzureStorageInfo(string account, string accountKey, string container)
        {
            StorageInfo = new AzureStorageInfo
            {
                Account = account,
                AccountKey = accountKey,
                Container = container
            };
        }

        public AzureStorageInfo GetAzureStorageInfo()
        {
            return StorageInfo;
        }

        public virtual AppManifestBase GetManifest()
        {
            return null;
        }

        public static AppManifestBase GetManifestFromJson(string json)
        {
            var manifestJson = "";
            if (!string.IsNullOrEmpty(json))
            {
                manifestJson = json;
            }
            var js = new JavaScriptSerializer();
            var appManifest = (AppManifestBase) js.Deserialize(manifestJson, typeof(AppManifestBase));
            return appManifest;
        }

        public static AppManifestBase GetManifestFromAzureStorage(string storageAccount, string accountKey,
            string container)
        {
            //Get or create the container
            var blobStorage = new BlobStorage(storageAccount, accountKey, container);
            var manifestJson = string.Empty;

            //Load or create a new manifest
            AppManifestBase appManifest;
            try
            {
                manifestJson = blobStorage.DownloadText("manifest.json");
            }
            catch
            {
                // ignored
            }
            if (string.IsNullOrEmpty(manifestJson))
            {
                appManifest = new AppManifestBase();
            }
            else
            {
                var js = new JavaScriptSerializer();
                appManifest = (AppManifestBase) js.Deserialize(manifestJson, typeof(AppManifestBase));
            }

            //Set the storage info
            appManifest.SetAzureStorageInfo(storageAccount, accountKey, container);
            return appManifest;
        }

        public static void SaveManifestToAzureStorage(AppManifestBase appManifest)
        {
            var azureStorageInfo = appManifest.GetAzureStorageInfo();
            if (azureStorageInfo == null) return;

            var blobStorage = new BlobStorage(azureStorageInfo.Account, azureStorageInfo.AccountKey,
                azureStorageInfo.Container);
            var js = new JavaScriptSerializer();

            var json = js.Serialize(appManifest);
            blobStorage.UploadText(json, "manifest.json");
        }

        public static void SaveManifestToFileSystem(AppManifestBase appManifest)
        {
            if (string.IsNullOrEmpty(appManifest.BaseFilePath)) return;

            if (!appManifest.BaseFilePath.EndsWith(@"\")) appManifest.BaseFilePath += @"\";

            var js = new JavaScriptSerializer();

            var json = js.Serialize(appManifest);
            File.WriteAllText(appManifest.BaseFilePath + "manifest.json", json);
        }
    }
}