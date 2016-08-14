using System;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class FeatureRemoverCreator
    {
        public virtual Guid FeatureId { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual FeatureDefinitionScope FeatureDefinitionScope { get; set; }
        public virtual bool Force { get; set; }
    }
}