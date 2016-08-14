using System;
using System.Collections.Generic;
using System.Linq;
using IQAppProvisioningBaseClasses.Events;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;
using File = System.IO.File;

// This provisioner is intended for use by the PowerShell module

namespace IQAppSiteProvisioner
{
    public class Provisioner : ProvisioningManagerBase
    {
        public void Deprovision(ClientContext ctx, Web web, string siteDefinitionJsonFileAbsolutePath)
        {
            try
            {
                var json = File.ReadAllText(siteDefinitionJsonFileAbsolutePath);

                //TODO: Deal with fallout from Version problem
                var siteDef = SiteDefinition.GetSiteDefinitionFromJson(json);

                Deprovision(ctx, web, siteDef);
            }
            catch (Exception ex)
            {
                var newEx = new Exception("Error during deprovisioning", ex);
                throw newEx;
            }
        }

        public void Deprovision(ClientContext ctx, Web web, SiteDefinition siteDef)
        {
            if (!web.IsPropertyAvailable("ServerRelativeUrl"))
            {
                //This fetches all of the Web's properties
                //You might be tempted to restrict this by just the properties you know
                //you need right now, but doing that will ensure that either this class has 
                //to be edited every time something in the call stack needs a new property
                //or many round trips for information about the web over the course of the whole operation.
                //The PowerShell module loads all properties.
                ctx.Load(web);
                ctx.ExecuteQueryRetry();
            }
            if (siteDef.WebDefinition?.Webs == null || siteDef.WebDefinition.Webs.Count == 0)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, "No webs defined to delete");
                return;
            }

            //Strip off any starting / WebCreationInformation.Url does not like it
            if (siteDef.WebDefinition.Url.StartsWith("/"))
                siteDef.WebDefinition.Url = siteDef.WebDefinition.Url.Substring(1);
            if (siteDef.WebDefinition.Url != "")
            {
                throw new InvalidOperationException("Root of siteDefinition must be '/' or empty");
            }
            var baseUrl = web.ServerRelativeUrl;

            DeleteChildWebs(ctx, web, baseUrl, siteDef.WebDefinition.Webs.Values.ToList());
            var manifestProvisioner = new IQAppManifestProvisioner.Provisioner();
            manifestProvisioner.WriteNotificationsToStdOut = WriteNotificationsToStdOut;
            manifestProvisioner.Notify += Provisioner_Notify;
            manifestProvisioner.Deprovision(ctx, web, siteDef.WebDefinition.AppManifest);
        }

        private void DeleteChildWebs(ClientContext ctx, Web currentWeb, string baseUrl,
            List<WebCreator> webs)
        {
            foreach (var webDef in webs)
            {
                var webUrl = webDef.Url;
                if (!webDef.Url.StartsWith("/") && baseUrl != "/") webUrl = "/" + webUrl;
                webUrl = baseUrl + webUrl;

                ctx.Load(currentWeb.Webs, ws => ws.Include(w => w.ServerRelativeUrl));
                ctx.ExecuteQueryRetry();
                var targetWeb =
                    currentWeb.Webs.SingleOrDefault(
                        w => w.ServerRelativeUrl.ToLowerInvariant() == webUrl.ToLowerInvariant());
                if (targetWeb != null)
                {
                    if (webDef.Webs != null)
                    {
                        DeleteChildWebs(ctx, targetWeb, webUrl, webDef.Webs.Values.ToList());
                    }
                    targetWeb.DeleteObject();
                    ctx.ExecuteQueryRetry();
                    OnNotify(ProvisioningNotificationLevels.Verbose, $"Deleted Web at {webUrl}");
                }
            }
        }

        public void Provision(ClientContext ctx, Web web, string siteDefinitionJsonFileAbsolutePath)
        {
            var json = File.ReadAllText(siteDefinitionJsonFileAbsolutePath);
            var siteDef = SiteDefinition.GetSiteDefinitionFromJson(json);
            Provision(ctx, web, siteDef);
        }

        public void Provision(ClientContext ctx, Web web, SiteDefinition siteDef)
        {
            if (!web.IsPropertyAvailable("ServerRelativeUrl"))
            {
                //This fetches all of the Web's properties
                //You might be tempted to restrict this by just the properties you know
                //you need right now, but doing that will ensure that either this class has 
                //to be edited every time something in the call stack needs a new property
                //or many round trips for information about the web over the course of the whole operation.
                //The PowerShell module loads all properties.
                ctx.Load(web);
                ctx.Load(ctx.Site.RootWeb);
                ctx.ExecuteQueryRetry();
            }

            var baseUrl = web.ServerRelativeUrl;

            if (siteDef.RootWebOnly)
            {
                var rootWeb = ctx.Site.RootWeb;

                if (baseUrl != rootWeb.ServerRelativeUrl)
                {
                    throw new InvalidOperationException("The web is not a root web as this site definition requires!");
                }
            }
            //TODO: Azure storage
            if (siteDef.StorageType != StorageTypes.FileSystem)
            {
                throw new InvalidOperationException("Site definition is not file system based!");
            }
            if (string.IsNullOrWhiteSpace(siteDef.BaseFilePath))
            {
                throw new InvalidOperationException("Site definition must specify the BaseFilePath!");
            }
            if (siteDef.WebDefinition != null)
            {
                //Strip off any starting / WebCreationInformation.Url does not like it
                if (siteDef.WebDefinition.Url.StartsWith("/"))
                    siteDef.WebDefinition.Url = siteDef.WebDefinition.Url.Substring(1);
                if (siteDef.WebDefinition.Url != "")
                {
                    throw new InvalidOperationException("Root of siteDefinition must be '/' or empty");
                }
                ApplyWeb(siteDef.WebDefinition, baseUrl, ctx, web);
            }
        }

        private void ApplyWeb(WebCreator webDefinition, string baseUrl, ClientContext ctx, Web currentWeb)
        {
            var webUrl = webDefinition.Url;
            if (!webDefinition.Url.StartsWith("/") && baseUrl != "/") webUrl = "/" + webUrl;
            webUrl = baseUrl + webUrl;

            if (webDefinition.Url != string.Empty) //Not a root web
            {
                ctx.Load(currentWeb.Webs, ws => ws.Include(w => w.ServerRelativeUrl));
                ctx.ExecuteQueryRetry();
                var targetWeb =
                    currentWeb.Webs.SingleOrDefault(
                        w => w.ServerRelativeUrl.ToLowerInvariant() == webUrl.ToLowerInvariant());
                if (targetWeb == null)
                {
                    var newWeb = CreateWeb(webDefinition, ctx, currentWeb, webUrl);
                    CreateWebs(webDefinition, ctx, webUrl, newWeb);
                }
                else
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, $"{webUrl} already exists!");
                    CreateWebs(webDefinition, ctx, webUrl, targetWeb);
                }
            }
            else
            {
                CreateWebs(webDefinition, ctx, webUrl, currentWeb);
            }
            var manifestProvisioner = new IQAppManifestProvisioner.Provisioner();
            manifestProvisioner.WriteNotificationsToStdOut = WriteNotificationsToStdOut;
            manifestProvisioner.Notify += Provisioner_Notify;
            manifestProvisioner.Provision(ctx, currentWeb, webDefinition.AppManifest);
        }

        private void CreateWebs(WebCreator webDefinition, ClientContext ctx, string webUrl, Web web)
        {
            if (webDefinition.Webs != null && webDefinition.Webs.Count > 0)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, $"Creating webs for {web.ServerRelativeUrl}");
                foreach (var wd in webDefinition.Webs.Values)
                {
                    ApplyWeb(wd, webUrl, ctx, web);
                }
            }
        }

        private Web CreateWeb(WebCreator webDefinition, ClientContext ctx, Web currentWeb, string webUrl)
        {
            OnNotify(ProvisioningNotificationLevels.Verbose, $"Creating web at {webUrl}");
            var useSamePermissionsAsParentSite = !(webDefinition.SecurityConfiguration != null && webDefinition.SecurityConfiguration.BreakInheritance);
            var wci = new WebCreationInformation
            {
                Title = webDefinition.Title,
                Description = webDefinition.Description,
                Language = (int)webDefinition.Language,
                Url = webDefinition.Url,
                UseSamePermissionsAsParentSite = useSamePermissionsAsParentSite,
                WebTemplate = webDefinition.WebTemplate
            };
            var newWeb = currentWeb.Webs.Add(wci);
            ctx.Load(newWeb);
            ctx.ExecuteQueryRetry();
            OnNotify(ProvisioningNotificationLevels.Verbose, $"Created web at {webUrl}");
            return newWeb;
        }

        private void Provisioner_Notify(object sender, IQAppProvisioningBaseClasses.Events.ProvisioningNotificationEventArgs eventArgs)
        {
            OnNotify(eventArgs.Level, eventArgs.Detail);
        }
    }
}