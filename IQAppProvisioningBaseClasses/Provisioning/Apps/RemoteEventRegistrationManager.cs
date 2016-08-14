using System.Collections.Generic;
using System.Diagnostics;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class RemoteEventRegistrationManager : ProvisioningManagerBase
    {
        public virtual List<RemoteEventRegistrationCreator> RemoteEventRegistrationCreators { get; set; }

        public virtual void CreateEventHandlers(ClientContext clientContext, Web web,
            List<RemoteEventRegistrationCreator> remoteEventRegistrationCreators, string remoteHost)
        {
            if (remoteEventRegistrationCreators == null || remoteEventRegistrationCreators.Count == 0) return;
            Trace.TraceInformation("Attaching event handlers at web");

            var baseEndpointUrl = "https://" + remoteHost;

            foreach (var creator in RemoteEventRegistrationCreators)
            {
                var handlerEndpointUrl = baseEndpointUrl + creator.EndpointUrl;
                if (string.IsNullOrEmpty(creator.ListTitle))
                {
                    AttachEventHandler(handlerEndpointUrl, web, creator.Eventname, creator.EventReceiverType,
                        clientContext);
                }
                else
                {
                    var list = web.Lists.GetByTitle(creator.ListTitle);
                    AttachEventHandler(handlerEndpointUrl, list, creator.Eventname, creator.EventReceiverType,
                        clientContext);
                }
            }

            clientContext.ExecuteQueryRetry();
        }

        public virtual void CreateEventHandlers(ClientContext clientContext, List list,
            List<RemoteEventRegistrationCreator> remoteEventRegistrationCreators, string remoteHost)
        {
            Trace.TraceInformation("Attaching list event handlers");

            var baseEndpointUrl = "https://" + remoteHost;

            foreach (var creator in RemoteEventRegistrationCreators)
            {
                var handlerEndpointUrl = baseEndpointUrl + creator.EndpointUrl;
                AttachEventHandler(handlerEndpointUrl, list, creator.Eventname, creator.EventReceiverType, clientContext);
            }

            clientContext.ExecuteQueryRetry();
        }

        private void AttachEventHandler(string handlerEndpoint, List list, string name, EventReceiverType receiverType,
            ClientContext clientContext)
        {
            clientContext.Load(list, l => l.Title, l => l.EventReceivers.Include(e => e.ReceiverName));
            clientContext.ExecuteQueryRetry();

            foreach (var eventReciever in list.EventReceivers)
            {
                if (eventReciever.ReceiverName == name) return;
            }

            var eventReceiver = new EventReceiverDefinitionCreationInformation
            {
                EventType = receiverType,
                ReceiverName = name,
                ReceiverUrl = handlerEndpoint,
                SequenceNumber = 10000,
                Synchronization = EventReceiverSynchronization.Asynchronous
            };

            list.EventReceivers.Add(eventReceiver);
            OnNotify(ProvisioningNotificationLevels.Verbose,
                "Attaching remote event handler to list " + list.Title + " | " + name);
        }

        private void AttachEventHandler(string handlerEndpoint, Web web, string name, EventReceiverType receiverType,
            ClientContext clientContext)
        {
            clientContext.Load(web, w => w.EventReceivers.Include(e => e.ReceiverName));
            clientContext.ExecuteQueryRetry();

            foreach (var eventReciever in web.EventReceivers)
            {
                if (eventReciever.ReceiverName == name) return;
            }

            var eventReceiver = new EventReceiverDefinitionCreationInformation
            {
                EventType = receiverType,
                ReceiverName = name,
                ReceiverUrl = handlerEndpoint,
                SequenceNumber = 10000,
                Synchronization = EventReceiverSynchronization.Asynchronous
            };

            web.EventReceivers.Add(eventReceiver);
            OnNotify(ProvisioningNotificationLevels.Verbose, "Attaching remote event handler to web | " + name);
        }
    }
}