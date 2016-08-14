using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    public class RoleDefinitionCreatorBuilder : CreatorBuilderBase
    {
        public string GetRoleDefinitionCreator(ClientContext ctx, string roleDefinitionName)
        {
            var manifest = new AppManifestBase();
            GetRoleDefinitionCreator(ctx, roleDefinitionName, manifest);
            if (manifest.RoleDefinitions.ContainsKey(roleDefinitionName))
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.RoleDefinitions[roleDefinitionName]);
            }
            OnVerboseNotify("NO INFORMATION FOUND FOR " + roleDefinitionName);
            return string.Empty;
        }

        public void GetRoleDefinitionCreator(ClientContext ctx, string roleName, AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingRoleDefinitions = manifest.RoleDefinitions;
            existingRoleDefinitions = existingRoleDefinitions ?? new Dictionary<string, RoleDefinitionCreator>();


            var creator = GetRoleDefinitionFromSite(ctx, roleName);
            if (creator != null)
            {
                OnVerboseNotify($"Got field creation information for {roleName}");
                existingRoleDefinitions[roleName] = creator;
            }
            else
            {
                OnVerboseNotify($"NO INFORMATION FOUND FOR {roleName}");
            }
            manifest.RoleDefinitions = existingRoleDefinitions;
        }

        public string GetRoleDefinitionCreators(ClientContext ctx)
        {
            var manifest = new AppManifestBase();
            GetRoleDefinitionCreators(ctx, manifest);

            var js = new JavaScriptSerializer();
            return js.Serialize(manifest.RoleDefinitions);
        }

        public void GetRoleDefinitionCreators(ClientContext ctx, AppManifestBase manifest)
        {
            if (manifest == null) return;
            manifest.RoleDefinitions = GetRoleDefinitionsFromSite(ctx);
        }

        private Dictionary<string, RoleDefinitionCreator> GetRoleDefinitionsFromSite(ClientContext ctx)
        {
            OnVerboseNotify("Getting role definition from site root web");

            var retVal = new Dictionary<string, RoleDefinitionCreator>();

            var roleDefinitions = ctx.Web.RoleDefinitions;
            ctx.Load(roleDefinitions);

            ctx.ExecuteQueryRetry();

            foreach (var roleDefinition in roleDefinitions)
            {
                retVal[roleDefinition.Name] = new RoleDefinitionCreator
                {
                    Name = roleDefinition.Name,
                    Description = roleDefinition.Description,
                    Order = roleDefinition.Order,
                    BasePermissions = GetPermissionKinds(roleDefinition.BasePermissions)
                };
            }

            return retVal;
        }

        private RoleDefinitionCreator GetRoleDefinitionFromSite(ClientContext ctx, string roleName)
        {
            OnVerboseNotify("Getting role definition from site root web");

            var retVal = new RoleDefinitionCreator();

            var roleDefinition = ctx.Web.RoleDefinitions.GetByName(roleName);
            ctx.Load(roleDefinition);
            try
            {
                ctx.ExecuteQueryRetry();

                retVal.Name = roleDefinition.Name;
                retVal.Description = roleDefinition.Description;
                retVal.Order = roleDefinition.Order;

                retVal.BasePermissions = GetPermissionKinds(roleDefinition.BasePermissions);

                return retVal;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private List<PermissionKind> GetPermissionKinds(BasePermissions basePermissions)
        {
            var retVal = new List<PermissionKind>();
            foreach (var permissionKind in (PermissionKind[]) Enum.GetValues(typeof(PermissionKind)))
            {
                if (basePermissions.Has(permissionKind))
                {
                    retVal.Add(permissionKind);
                }
            }
            return retVal;
        }
    }
}