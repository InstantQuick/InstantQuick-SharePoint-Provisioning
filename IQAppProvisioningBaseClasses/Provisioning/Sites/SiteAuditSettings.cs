using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class SiteAuditSettings
    {
        public AuditMaskType AuditMaskType { get; set; }
        public int AuditLogTrimmingRetention { get; set; }
        public bool TrimAuditLog { get; set; }
    }
}
