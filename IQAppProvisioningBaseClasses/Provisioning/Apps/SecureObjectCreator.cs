using System.Collections.Generic;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public enum SecureObjectType
    {
        Web,
        List,
        File
    }

    public class SecureObjectCreator
    {
        public virtual SecureObjectType SecureObjectType { get; set; }
        public virtual SecurableObject SecurableObject { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool BreakInheritance { get; set; }
        public bool ResetChildPermissions { get; set; }
        public bool CopyExisting { get; set; }
        public Dictionary<string, string> GroupRoleDefinitions { get; set; }
    }
}