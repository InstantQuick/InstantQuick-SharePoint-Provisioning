using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class SiteDefinition
    {
        //Not a property to avoid JSON serialization
        internal AzureStorageInfo StorageInfo;
        //End not a property to avoid JSON serialization

        /// <summary>
        ///     Represents the local file system base file path
        /// </summary>
        public virtual string BaseFilePath { get; set; }

        /// <summary>
        ///     Description of the site definition
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        ///     This is the WebCreator to apply to the top web where the site definition is applied
        ///     This is not necessarily the root web of a site collection
        /// </summary>
        public virtual WebCreator WebDefinition { get; set; }

        /// <summary>
        ///     If true the provisioner will reject attempts to apply this template to anything other than a site collection root
        /// </summary>
        public virtual bool RootWebOnly { get; set; }

        /// <summary>
        ///     Unique Identifier of the site defintion
        ///     If using Azure storage, this should be a guid
        ///     The container name will be (site definition guid + version).replace(".", "_")
        /// </summary>
        public virtual string SiteDefinitionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        ///     The type of storage that contains the site def and its resources
        /// </summary>
        public virtual StorageTypes StorageType { get; set; } = StorageTypes.FileSystem;

        /// <summary>
        ///     Human friendly name of the site defintion
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        ///     Version in proper System.Version parseable format
        /// </summary>
        public virtual string Version { get; set; } = "0.0.0.0";

        /// <summary>
        ///     Sets the current storage properties which are not subject to serialization
        /// </summary>
        /// <param name="account">Azure storage account name</param>
        /// <param name="accountKey">Azure storage account key</param>
        /// <param name="container">The container that contains the saved site definition</param>
        public void SetAzureStorageInfo(string account, string accountKey, string container)
        {
            StorageInfo = new AzureStorageInfo
            {
                Account = account,
                AccountKey = accountKey,
                Container = container
            };
        }

        /// <summary>
        ///     Fetches the current Azure storage config
        /// </summary>
        /// <returns></returns>
        public AzureStorageInfo GetAzureStorageInfo()
        {
            return StorageInfo;
        }

        /// <summary>
        ///     Factory method used by the PowerShell module
        /// </summary>
        /// <param name="json">Valid JSON of a serialized IQAppProvisioningBaseClasses.Provisioning.SiteDefition</param>
        /// <returns></returns>
        public static SiteDefinition GetSiteDefinitionFromJson(string json)
        {
            var manifestJson = "";
            if (!string.IsNullOrEmpty(json))
            {
                manifestJson = json;
            }
            var js = new JavaScriptSerializer();
            var siteCreator = (SiteDefinition) js.Deserialize(manifestJson, typeof(SiteDefinition));
            return siteCreator;
        }

        /// <summary>
        ///     Factory method used by the PowerShell module
        ///     Either loads a site definition or creates a new one with the provided storage information
        /// </summary>
        /// <param name="storageAccount">Azure storage account name</param>
        /// <param name="accountKey">Azure storage account key</param>
        /// <param name="container">The container that contains the saved site definition</param>
        /// <returns></returns>
        public static SiteDefinition GetSiteDefinitionFromAzureStorage(string storageAccount, string accountKey,
            string container)
        {
            //Get or create the container
            var blobStorage = new BlobStorage(storageAccount, accountKey, container);
            var manifestJson = string.Empty;

            //Load or create a new manifest
            SiteDefinition siteCreator;
            try
            {
                manifestJson = blobStorage.DownloadText("sitedefinition.json");
            }
            catch
            {
                // ignored
            }
            if (string.IsNullOrEmpty(manifestJson))
            {
                siteCreator = new SiteDefinition();
            }
            else
            {
                var js = new JavaScriptSerializer();
                siteCreator = (SiteDefinition) js.Deserialize(manifestJson, typeof(SiteDefinition));
            }

            //Set the storage info
            siteCreator.SetAzureStorageInfo(storageAccount, accountKey, container);
            return siteCreator;
        }

        /// <summary>
        ///     Saves sitedefinition.json to Azure storage
        /// </summary>
        /// <param name="siteCreator"></param>
        public static void SaveSiteDefinitionToAzureStorage(SiteDefinition siteCreator)
        {
            var azureStorageInfo = siteCreator.GetAzureStorageInfo();
            if (azureStorageInfo == null) return;

            var blobStorage = new BlobStorage(azureStorageInfo.Account, azureStorageInfo.AccountKey,
                azureStorageInfo.Container);
            var js = new JavaScriptSerializer();

            var json = js.Serialize(siteCreator);
            blobStorage.UploadText(json, "sitedefinition.json");
        }

        /// <summary>
        ///     Saves sitedefinition.json to BaseFilePath
        /// </summary>
        /// <param name="siteCreator"></param>
        public static void SaveSiteDefinitionToFileSystem(SiteDefinition siteCreator)
        {
            if (string.IsNullOrEmpty(siteCreator.BaseFilePath)) return;

            if (!siteCreator.BaseFilePath.EndsWith(@"\")) siteCreator.BaseFilePath += @"\";

            var js = new JavaScriptSerializer();
            var json = js.Serialize(siteCreator);
            File.WriteAllText(siteCreator.BaseFilePath + "sitedefinition.json", json, new System.Text.UTF8Encoding());
        }
    }
}