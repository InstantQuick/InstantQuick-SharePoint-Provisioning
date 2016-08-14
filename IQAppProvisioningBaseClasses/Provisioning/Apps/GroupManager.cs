using System.Collections.Generic;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class GroupManager : ProvisioningManagerBase
    {
        public virtual Dictionary<string, GroupCreator> GroupCreators { get; set; }

        public virtual void ProvisionGroups(ClientContext ctx)
        {
            ProvisionGroups(ctx, ctx.Web);
        }

        public virtual void ProvisionGroups(ClientContext ctx, Web web)
        {
            if (GroupCreators == null) return;

            var groups = web.SiteGroups;
            ctx.Load(groups, g => g.Include
                (group => group.Title));
            ctx.ExecuteQueryRetry();

            var existingGroups = new Dictionary<string, Group>();
            foreach (var group in groups)
            {
                existingGroups.Add(group.Title, group);
            }

            var added = false;
            foreach (var key in GroupCreators.Keys)
            {
                if (!existingGroups.ContainsKey(key))
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Creating group " + GroupCreators[key].Title);
                    var groupInfo = new GroupCreationInformation
                    {
                        Title = GroupCreators[key].Title,
                        Description = GroupCreators[key].Description
                    };
                    GroupCreators[key].Group = web.SiteGroups.Add(groupInfo);
                    ctx.Load(GroupCreators[key].Group);
                    added = true;
                }
                else
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose,
                        "Group " + GroupCreators[key].Title + " exists. Skipping");
                }
            }
            if (added) ctx.ExecuteQueryRetry();
            else return;

            foreach (var groupCreator in GroupCreators.Values)
            {
                if (groupCreator.Group != null)
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Setting properties for " + groupCreator.Title);
                    groupCreator.Group.AllowMembersEditMembership = groupCreator.AllowMembersEditMembership;
                    groupCreator.Group.AllowRequestToJoinLeave = groupCreator.AllowRequestToJoinLeave;
                    groupCreator.Group.AutoAcceptRequestToJoinLeave = groupCreator.AutoAcceptRequestToJoinLeave;
                    groupCreator.Group.OnlyAllowMembersViewMembership = groupCreator.OnlyAllowMembersViewMembership;
                    groupCreator.Group.Update();
                    ctx.ExecuteQueryRetry();
                }
            }
        }
    }
}