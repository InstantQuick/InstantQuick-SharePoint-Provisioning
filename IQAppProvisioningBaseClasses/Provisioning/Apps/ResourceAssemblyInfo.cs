namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ResourceAssemblyInfo
    {
        //If empty provisioning uses the role config default 
        //(ClientCatalogBlobContainer in settings)
        public string BlobContainer { get; set; }
        public string AssemblyName { get; set; }
    }
}