using System.Collections.Generic;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class RoleDefinitionManager : ProvisioningManagerBase
    {
        private readonly ClientContext _ctx;
        private readonly Web _web;

        public RoleDefinitionManager()
        {
        }

        public RoleDefinitionManager(ClientContext ctx) : this(ctx, ctx.Web)
        {
        }

        public RoleDefinitionManager(ClientContext ctx, Web web)
        {
            _ctx = ctx;
            _web = web;
            _ctx.Load(web.RoleDefinitions);
            _ctx.ExecuteQueryRetry();
        }

        public virtual Dictionary<string, RoleDefinitionCreator> RoleDefinitions { get; set; }

        public virtual void Provision()
        {
            if (RoleDefinitions != null)
            {
                var existingRoleDefinitions = new List<string>();
                foreach (var roleDef in _web.RoleDefinitions)
                {
                    existingRoleDefinitions.Add(roleDef.Name);
                }

                foreach (var key in RoleDefinitions.Keys)
                {
                    if (!existingRoleDefinitions.Contains(key))
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose, "Creating role definition " + key);
                        var creator = new RoleDefinitionCreationInformation
                        {
                            Name = RoleDefinitions[key].Name,
                            Description = RoleDefinitions[key].Description
                        };
                        var perms = new BasePermissions();
                        foreach (var p in RoleDefinitions[key].BasePermissions)
                        {
                            perms.Set(p);
                        }

                        creator.BasePermissions = perms;
                        creator.Order = RoleDefinitions[key].Order;
                        _web.RoleDefinitions.Add(creator);
                    }
                    else
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose, "Role definition " + key + " exists. Skipping");
                    }
                }
                _ctx.ExecuteQueryRetry();
            }
        }
    }
}