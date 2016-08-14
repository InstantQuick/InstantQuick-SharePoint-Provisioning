using System.Collections.Generic;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class RoleDefinitionCreator
    {
        public virtual List<PermissionKind> BasePermissions { get; set; }
        public virtual string Description { get; set; }
        public virtual string Name { get; set; }
        public virtual int Order { get; set; }
    }
}