using System.Collections.Generic;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class SecureObjectManager : ProvisioningManagerBase
    {
        private readonly ClientContext _ctx;

        public SecureObjectManager(ClientContext ctx)
        {
            _ctx = ctx;
            _ctx.Load(_ctx.Web, w => w.ServerRelativeUrl, w => w.SiteGroups, w => w.RoleDefinitions);
            _ctx.ExecuteQueryRetry();
        }

        public virtual List<SecureObjectCreator> SecureObjects { get; set; }

        public virtual void ApplySecurity()
        {
            foreach (var secureObject in SecureObjects)
            {
                switch (secureObject.SecureObjectType)
                {
                    case SecureObjectType.Web:
                        SecureWeb(secureObject);
                        break;
                    case SecureObjectType.List:
                        SecureList(secureObject);
                        break;
                    case SecureObjectType.File:
                        SecureFile(secureObject);
                        break;
                }
            }
        }

        private void SecureFile(SecureObjectCreator secureObject)
        {
            ListItem item;
            if (secureObject.SecurableObject != null)
            {
                item = (ListItem) secureObject.SecurableObject;
            }
            else
            {
                var folder = _ctx.Web.GetFolderByServerRelativeUrl(secureObject.Url);
                var file = folder.Files.GetByUrl(secureObject.Title);
                _ctx.Load(file, f => f.ListItemAllFields);
                _ctx.ExecuteQueryRetry();
                item = file.ListItemAllFields;
            }

            SetInheritance(item, secureObject);
            item.Update();
            _ctx.Load(item, i => i.RoleAssignments);
            _ctx.ExecuteQueryRetry();
            ApplyRoleAssignments(item, secureObject);
            item.Update();
            _ctx.ExecuteQueryRetry();
        }

        private void SecureList(SecureObjectCreator secureObject)
        {
            List list;
            if (secureObject.SecurableObject != null) list = (List) secureObject.SecurableObject;
            else list = _ctx.Web.Lists.GetByTitle(secureObject.Title);

            SetInheritance(list, secureObject);
            list.Update();
            _ctx.Load(list, i => i.RoleAssignments);
            _ctx.ExecuteQueryRetry();
            ApplyRoleAssignments(list, secureObject);
            list.Update();
            _ctx.ExecuteQueryRetry();
        }

        private void SecureWeb(SecureObjectCreator secureObject)
        {
            var webToSecure = _ctx.Site.OpenWeb(secureObject.Url);
            SetInheritance(webToSecure, secureObject);
            webToSecure.Update();
            _ctx.Load(webToSecure, i => i.RoleAssignments);
            _ctx.ExecuteQueryRetry();
            ApplyRoleAssignments(webToSecure, secureObject);
            webToSecure.Update();
            _ctx.ExecuteQueryRetry();
        }

        private void SetInheritance(SecurableObject objectToSecure, SecureObjectCreator definition)
        {
            if (definition.BreakInheritance)
            {
                objectToSecure.BreakRoleInheritance(definition.CopyExisting, definition.ResetChildPermissions);
            }
        }

        private void ApplyRoleAssignments(SecurableObject objectToSecure, SecureObjectCreator definition)
        {
            foreach (var key in definition.GroupRoleDefinitions.Keys)
            {
                Principal principal = null;
                if (key.StartsWith("c:"))
                {
                    principal = _ctx.Web.EnsureUser(key);
                }
                else
                {
                    if (key != "AssociatedMemberGroup" && key != "AssociatedOwnerGroup" &&
                        key != "AssociatedVisitorGroup")
                    {
                        principal = _ctx.Web.SiteGroups.GetByName(key);
                    }
                    else
                    {
                        _ctx.Load(_ctx.Web.AssociatedMemberGroup, g => g.Id);
                        _ctx.Load(_ctx.Web.AssociatedOwnerGroup, g => g.Id);
                        _ctx.Load(_ctx.Web.AssociatedVisitorGroup, g => g.Id);
                        _ctx.ExecuteQueryRetry();
                        switch (key)
                        {
                            case "AssociatedMemberGroup":
                                principal = _ctx.Web.AssociatedMemberGroup;
                                break;
                            case "AssociatedOwnerGroup":
                                principal = _ctx.Web.AssociatedOwnerGroup;
                                break;
                            case "AssociatedVisitorGroup":
                                principal = _ctx.Web.AssociatedVisitorGroup;
                                break;
                        }
                    }
                }
                var roleDef = _ctx.Web.RoleDefinitions.GetByName(definition.GroupRoleDefinitions[key]);
                var roleDefinitionBinding = new RoleDefinitionBindingCollection(_ctx) {roleDef};
                objectToSecure.RoleAssignments.Add(principal, roleDefinitionBinding);
            }
        }
    }
}