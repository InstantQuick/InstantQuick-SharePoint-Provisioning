namespace IQAppProvisioningBaseClasses.Provisioning
{
    /// <summary>
    /// Supports consolidated manifests where files are stored in different buckets
    /// </summary>
    public class ProvisioningFileCreator : FileCreator
    {
        internal AzureStorageInfo StorageInfo;

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
    }
}