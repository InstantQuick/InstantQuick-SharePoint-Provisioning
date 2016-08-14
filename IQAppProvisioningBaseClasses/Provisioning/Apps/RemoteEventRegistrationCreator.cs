using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class RemoteEventRegistrationCreator
    {
        public virtual string ListTitle { get; set; }
        public virtual string Eventname { get; set; }
        public virtual EventReceiverType EventReceiverType { get; set; }
        public virtual string EndpointUrl { get; set; }
    }
}