using System;
using System.Linq;
using System.Web.Script.Serialization;
using IQAppManifestBuilders;
using IQAppProvisioningBaseClasses;
using IQAppProvisioningBaseClasses.Provisioning;

namespace IQAppStorageMigrator
{
    public class Migrator : CreatorBuilderBase
    {
        public AppManifestBase MigrateFromFileSystemToAzure(AppManifestBase manifest, string storageAccount,
            string accountKey)
        {
            var js = new JavaScriptSerializer();

            //clone the manifest
            var migratedManifest = js.Deserialize<AppManifestBase>(js.Serialize(manifest));

            var containerName = migratedManifest.ManifestName.ToLower().Replace(' ', '-');

            if (!containerName.Replace("-", "").All(char.IsLetterOrDigit))
                throw new InvalidOperationException(containerName + " is not valid.");

            var blobStorage = new BlobStorage(storageAccount, accountKey,
                migratedManifest.ManifestName.ToLower().Replace(' ', '-'));

            OnVerboseNotify("Connected to Azure storage");

            migratedManifest.StorageType = StorageTypes.AzureStorage;

            if (migratedManifest.FileCreators != null && migratedManifest.FileCreators.Count > 0)
            {
                foreach (var fileCreator in migratedManifest.FileCreators)
                {
                    //Wiki pages don't have actual files to upload
                    if (fileCreator.Value.ContentType != "Wiki Page")
                    {
                        var blobName = fileCreator.Value.Url;
                        blobStorage.UploadFromFile(manifest.BaseFilePath + fileCreator.Value.RelativeFilePath, blobName);

                        OnVerboseNotify("Migrated " + blobName);
                    }
                }
            }

            migratedManifest.BaseFilePath = containerName;

            blobStorage.UploadText(js.Serialize(migratedManifest), "manifest.json");

            return migratedManifest;
        }
    }
}