using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

//Original from https://github.com/justazure/JustAzure.BlobStorage/blob/master/Part%203%20BlobMethods/BlobMethods/BlobMethods.cs
namespace IQAppProvisioningBaseClasses
{
    public class BlobStorage
    {
        private readonly string _storageAccountKey;

        private readonly string _storageAccountName;

        //this is the only public constructor; can't use this class without this info
        public BlobStorage(string storageAccountName, string storageAccountKey, string containerName)
        {
            _storageAccountName = storageAccountName;
            _storageAccountKey = storageAccountKey;

            CloudBlobContainer = SetUpContainer(storageAccountName, storageAccountKey, containerName);
        }

        //these variables are used throughout the class
        private CloudBlobContainer CloudBlobContainer { get; }

        private CloudBlobContainer SetUpContainer(string storageAccountName, string storageAccountKey,
            string containerName)
        {
            var connectionString =
                $@"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}";

            //get a reference to the container where you want to put the files
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            cloudBlobContainer.CreateIfNotExists();
            return cloudBlobContainer;
        }

        public string UploadFromFile(string localFilePath, string blobUrl)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(blobUrl);
            blob.UploadFromFile(localFilePath, FileMode.Open);
            return "Uploaded successfully.";
        }

        public string UploadText(string textToUpload, string targetFileName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(targetFileName);
            blob.UploadText(textToUpload);
            return "Finished uploading.";
        }

        public string UploadFromByteArray(byte[] uploadBytes, string targetFileName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(targetFileName);
            blob.UploadFromByteArray(uploadBytes, 0, uploadBytes.Length);
            return "Uploaded byte array successfully.";
        }

        public BlobProperties GetBlobProperties(string targetFileName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(targetFileName);
            return blob.Properties;
        }

        public DateTime? GetBlobLastModified(string targetFileName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(targetFileName);

            if (!blob.Exists()) return null;

            blob.FetchAttributes();

            return blob.Properties.LastModified?.DateTime;
        }

        public void DeleteBlob(string targetFileName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(targetFileName);

            if (blob.Exists()) blob.Delete();
        }

        public string UploadFromStream(Stream stream, string targetBlobName)
        {
            //reset the stream back to its starting point (no partial saves)
            stream.Position = 0;
            var blob = CloudBlobContainer.GetBlockBlobReference(targetBlobName);
            blob.UploadFromStream(stream);
            return "Uploaded successfully.";
        }

        public void CopyContainer(string destinationContainer)
        {
            var targetContainer = SetUpContainer(_storageAccountName, _storageAccountKey, destinationContainer);

            foreach (var iBlob in CloudBlobContainer.ListBlobs(useFlatBlobListing: true))
            {
                var blob = (CloudBlockBlob) iBlob;
                var targetBlob = targetContainer.GetBlockBlobReference(blob.Name);
                targetBlob.StartCopyFromBlob(blob);
            }
        }

        public void DeleteContainer()
        {
            CloudBlobContainer.Delete();
        }

        internal byte[] DownloadToByteArray(string targetFileName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(targetFileName);
            //you have to fetch the attributes to read the length
            blob.FetchAttributes();
            var fileByteLength = blob.Properties.Length;
            var myByteArray = new byte[fileByteLength];
            blob.DownloadToByteArray(myByteArray, 0);
            return myByteArray;
        }

        public string DownloadText(string blobName)
        {
            var blob = CloudBlobContainer.GetBlockBlobReference(blobName);
            return blob.DownloadText();
        }
    }
}