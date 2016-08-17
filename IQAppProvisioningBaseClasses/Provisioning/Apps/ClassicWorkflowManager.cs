using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ClassicWorkflowManager : ProvisioningManagerBase
    {
        public virtual Dictionary<string, ClassicWorkflowCreator> Creators { get; set; }

        public void CreateAll(ClientContext ctx)
        {
            CreateAll(ctx, ctx.Web);
        }

        public void CreateAll(ClientContext ctx, Web web)
        {
            if (Creators != null && Creators.Count > 0)
            {
                var webPartPageService = new WebPartPagesWebService.WebPartPagesWebService
                {
                    Url = web.Url + "/_vti_bin/webpartpages.asmx"
                };

                var credentials = ctx.Credentials as SharePointOnlineCredentials;
                if (credentials != null)
                {
                    var authCookieString = credentials.GetAuthenticationCookie(new Uri(web.Url));
                    string[] parts =
                    {
                        authCookieString.Substring(0, authCookieString.IndexOf('=')),
                        authCookieString.Substring(authCookieString.IndexOf('=') + 1)
                    };
                    webPartPageService.CookieContainer = new CookieContainer();
                    var cookie = new Cookie(parts[0], parts[1]) {Domain = new Uri(web.Url).Host};
                    webPartPageService.CookieContainer.Add(cookie);
                }
                else
                {
                    //Assumes the site is local intranet and that saved credentials exist in the credential manager 
                    webPartPageService.Credentials = ctx.Credentials;
                }
                try
                {
                    foreach (var def in Creators.Values)
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose,
                            "Associating workflow " + def.AssociateWorkflowMarkupConfigUrl);
                        webPartPageService.AssociateWorkflowMarkup(def.AssociateWorkflowMarkupConfigUrl,
                            def.ConfigVersion);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error registering workflows at " + web.Url + " | " + ex);
                    OnNotify(ProvisioningNotificationLevels.Normal,
                        "Error registering workflows at " + web.Url + " | " + ex);
                    throw;
                }
            }
        }

        public CustomActionCreator CreateAppWorkflowAssociationCustomAction(ClientContext ctx,
            Dictionary<string, ClassicWorkflowCreator> classicWorkflows, string userCustomActionTitle)
        {
            return CreateAppWorkflowAssociationCustomAction(ctx, ctx.Web, classicWorkflows, userCustomActionTitle);
        }

        public CustomActionCreator CreateAppWorkflowAssociationCustomAction(ClientContext ctx, Web web,
            Dictionary<string, ClassicWorkflowCreator> classicWorkflows, string userCustomActionTitle)
        {
            OnNotify(ProvisioningNotificationLevels.Normal, "Creating workflow registration bullet custom action");
            var creator = new CustomActionCreator
            {
                Location = "ScriptLink",
                Sequence = 9999,
                Title = userCustomActionTitle,
                RegistrationType = UserCustomActionRegistrationType.None
            };

            var js = new JavaScriptSerializer();
            var classicWorkflowsJson = js.Serialize(classicWorkflows);

            var customActionTemplate =
                Utility.GetFile("IQAppProvisioningBaseClasses.Resources.AppWorkflowAssociationCustomAction.min.js",
                    Assembly.GetExecutingAssembly());
            creator.ScriptBlock =
                customActionTemplate.Replace("{@WorkflowCreatorsJSON}", classicWorkflowsJson)
                    .Replace("{@WebUrl}", web.Url)
                    .Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl)
                    .Replace("{@UserCustomActionTitle}", userCustomActionTitle);

            return creator;
        }
    }
}