using System.Collections.Generic;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class NavigationCreator
    {
        public virtual bool UseShared { get; set; }
        public virtual bool ClearTopMenu { get; set; }
        public virtual bool ClearLeftMenu { get; set; }
        public virtual Dictionary<string, NavigationNodeCreator> TopNavigationNodes { get; set; }
        public virtual Dictionary<string, NavigationNodeCreator> LeftNavigationNodes { get; set; }
    }
}