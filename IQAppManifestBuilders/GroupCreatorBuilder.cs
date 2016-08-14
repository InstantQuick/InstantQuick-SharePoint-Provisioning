using System.Collections.Generic;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    public class GroupCreatorBuilder : CreatorBuilderBase
    {
        public string GetGroupCreator(ClientContext ctx, string groupName)
        {
            var manifest = new AppManifestBase();
            GetGroupCreator(ctx, groupName, manifest);
            if (manifest.GroupCreators.ContainsKey(groupName))
            {
                OnVerboseNotify($"Got group creation information for {groupName}");
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.GroupCreators[groupName]);
            }
            OnVerboseNotify("NO INFORMATION FOUND FOR " + groupName);
            return string.Empty;
        }

        public void GetGroupCreator(ClientContext ctx, string groupName, AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingGroups = manifest.GroupCreators;
            existingGroups = existingGroups ?? new Dictionary<string, GroupCreator>();


            var creator = GetGroupCreatorFromSite(ctx, groupName);
            if (creator != null)
            {
                OnVerboseNotify($"Got group creation information for {groupName}");
                existingGroups[groupName] = creator;
            }
            else
            {
                OnVerboseNotify("NO INFORMATION FOUND FOR " + groupName);
            }
            manifest.GroupCreators = existingGroups;
        }

        private GroupCreator GetGroupCreatorFromSite(ClientContext ctx, string groupName)
        {
            var retVal = new GroupCreator();

            var group = ctx.Web.SiteGroups.GetByName(groupName);
            ctx.Load(group);
            try
            {
                ctx.ExecuteQueryRetry();

                retVal.Title = group.Title;
                retVal.Description = group.Description;

                retVal.AllowMembersEditMembership = group.AllowMembersEditMembership;
                retVal.AllowRequestToJoinLeave = group.AllowRequestToJoinLeave;
                retVal.AutoAcceptRequestToJoinLeave = group.AutoAcceptRequestToJoinLeave;
                retVal.OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership;
                return retVal;
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}