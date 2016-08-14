using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    public class RemoteEventRegistrationCreatorBuilder : CreatorBuilderBase
    {
        public string GetRemoteEventRegistrationCreators(ClientContext ctx, Web web)
        {
            var manifest = new AppManifestBase();
            GetRemoteEventRegistrationCreators(ctx, web, manifest);
            if (manifest.RemoteEventRegistrationCreators != null)
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.RemoteEventRegistrationCreators);
            }
            OnVerboseNotify("No remote events found");
            return string.Empty;
        }

        public void GetRemoteEventRegistrationCreators(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingRemoteEventRegistrations = new List<RemoteEventRegistrationCreator>();

            GetRemoteEventRegistrationCreatorsFromWeb(ctx, web, existingRemoteEventRegistrations, manifest);

            manifest.RemoteEventRegistrationCreators = existingRemoteEventRegistrations;
        }

        private void GetRemoteEventRegistrationCreatorsFromWeb(ClientContext ctx, Web web,
            List<RemoteEventRegistrationCreator> existingRemoteEventRegistrations, AppManifestBase manifest)
        {
            var remoteEvents = web.EventReceivers;
            ctx.Load(remoteEvents,
                events =>
                    events.Include(e => e.ReceiverName, e => e.EventType, e => e.ReceiverUrl)
                        .Where(e => e.ReceiverUrl != null && e.ReceiverUrl != string.Empty));
            ctx.ExecuteQueryRetry();
            foreach (var remoteEvent in remoteEvents)
            {
                var newCreator = new RemoteEventRegistrationCreator
                {
                    Eventname = remoteEvent.ReceiverName,
                    EventReceiverType = remoteEvent.EventType
                };
                var uri = new Uri(remoteEvent.ReceiverUrl);
                newCreator.EndpointUrl = uri.AbsolutePath;
                if (string.IsNullOrEmpty(manifest.RemoteHost)) manifest.RemoteHost = uri.Host;
                existingRemoteEventRegistrations.Add(newCreator);
            }
        }

        public string GetRemoteEventRegistrationCreators(ClientContext ctx, List list, string listTitle)
        {
            var manifest = new AppManifestBase
            {
                ListCreators = new Dictionary<string, ListCreator> {[listTitle] = new ListCreator()}
            };
            GetRemoteEventRegistrationCreators(ctx, list, listTitle, manifest);
            var js = new JavaScriptSerializer();
            return js.Serialize(manifest.ListCreators[listTitle].RemoteEventRegistrationCreators);
        }

        public void GetRemoteEventRegistrationCreators(ClientContext ctx, List list, string listTitle,
            AppManifestBase manifest)
        {
            if (manifest?.ListCreators == null || !manifest.ListCreators.ContainsKey(listTitle) || list == null ||
                string.IsNullOrEmpty(listTitle)) return;

            var existingRemoteEventRegistrations = new List<RemoteEventRegistrationCreator>();

            GetRemoteEventRegistrationCreatorsFromList(ctx, list, existingRemoteEventRegistrations, manifest);
            manifest.ListCreators[listTitle].RemoteEventRegistrationCreators = existingRemoteEventRegistrations;
        }

        private void GetRemoteEventRegistrationCreatorsFromList(ClientContext ctx, List list,
            List<RemoteEventRegistrationCreator> existingRemoteEventRegistrations, AppManifestBase manifest)
        {
            var remoteEvents = list.EventReceivers;
            ctx.Load(remoteEvents,
                events =>
                    events.Include(e => e.ReceiverName, e => e.EventType, e => e.ReceiverUrl)
                        .Where(e => e.ReceiverUrl != null && e.ReceiverUrl != string.Empty));
            ctx.ExecuteQueryRetry();
            foreach (var remoteEvent in remoteEvents)
            {
                var newCreator = new RemoteEventRegistrationCreator
                {
                    Eventname = remoteEvent.ReceiverName,
                    EventReceiverType = remoteEvent.EventType
                };
                var uri = new Uri(remoteEvent.ReceiverUrl);
                newCreator.EndpointUrl = uri.AbsolutePath;
                if (string.IsNullOrEmpty(manifest.RemoteHost)) manifest.RemoteHost = uri.Host;
                existingRemoteEventRegistrations.Add(newCreator);
            }
        }
    }
}