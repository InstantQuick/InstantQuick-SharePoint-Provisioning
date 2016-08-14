using System.Collections.Generic;
using IQAppRuntimeResources;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class PageResource
    {
        public string Url { get; set; }
        public bool PrependWebServerRelativeUrl { get; set; }
        public ResourceTypes ResourceType { get; set; }
        public bool Wait { get; set; }
    }

    public class ClientPageRuntimeResources
    {
        public List<PageResource> Scripts { get; set; }
        public List<PageResource> StyleSheets { get; set; }
    }
}