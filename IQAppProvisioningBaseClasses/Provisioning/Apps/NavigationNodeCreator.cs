using System.Collections.Generic;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class NavigationNodeCreator
    {
        public virtual bool AsLastNode { get; set; }
        public virtual bool IsExternal { get; set; }
        public virtual string Title { get; set; }
        public virtual string Url { get; set; }
        public virtual List<NavigationNodeCreator> Children { get; set; }
    }
}