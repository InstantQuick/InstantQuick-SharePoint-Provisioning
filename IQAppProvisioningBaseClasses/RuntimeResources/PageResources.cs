using System.Collections.Generic;

namespace IQAppRuntimeResources
{
    public enum ResourceTypes
    {
        Script,
        Css
    }

    public class DataPageResource
    {
        public DataPageResource()
        {
            Resources = new List<PageResource>();
        }

        public string Alias { get; set; }
        public string EmbeddedResourceKey { get; set; }
        public bool PageInitializationScript { get; set; }
        public bool PageScript { get; set; }
        public ResourceTypes ResourceType { get; set; }
        public List<PageResource> Resources { get; set; }
        public string StorageContainer { get; set; }
    }

    public class PageResource
    {
        public string Url { get; set; }
        public string FullUrl { get; set; }
        public string EmbeddedResourceKey { get; set; }
        public bool Wait { get; set; }
        public ResourceTypes ResourceType { get; set; }
        public bool ExternalResource { get; set; }
    }
}