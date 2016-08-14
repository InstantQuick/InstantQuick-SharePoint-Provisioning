using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class GroupCreator
    {
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public Group Group { get; set; }
        public virtual bool AllowMembersEditMembership { get; set; }
        public virtual bool AllowRequestToJoinLeave { get; set; }
        public virtual bool AutoAcceptRequestToJoinLeave { get; set; }
        public virtual bool OnlyAllowMembersViewMembership { get; set; }
    }
}