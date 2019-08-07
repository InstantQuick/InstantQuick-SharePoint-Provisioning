using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.Azure;

namespace IQAppManifestBuilders
{
    public class AzureStorageConfig
    {
        public string StorageAccount { get; set; }
        public string StorageAccountKey { get; set; }

        public static AzureStorageConfig Get()
        {
            var storageAccount = CloudConfigurationManager.GetSetting("DefaultBlobStorageAccount");
            var storageAccountKey = CloudConfigurationManager.GetSetting("DefaultBlobStorageAccountKey");

            return new AzureStorageConfig
            {
                StorageAccount = storageAccount,
                StorageAccountKey = storageAccountKey
            };
        }
    }

    public class ManifestStorage
    {
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

        public static void CopyContainer(AzureStorageConfig azureStorageInfo, string sourceContainer,
            string destinationContainer)
        {
            var blobStorage = new BlobStorage(azureStorageInfo.StorageAccount, azureStorageInfo.StorageAccountKey,
                sourceContainer);

            blobStorage.CopyContainer(destinationContainer);
        }

        public static void DeleteContainer(AzureStorageConfig azureStorageInfo, string container)
        {
            var blobStorage = new BlobStorage(azureStorageInfo.StorageAccount, azureStorageInfo.StorageAccountKey,
                container);

            blobStorage.DeleteContainer();
        }
    }
}