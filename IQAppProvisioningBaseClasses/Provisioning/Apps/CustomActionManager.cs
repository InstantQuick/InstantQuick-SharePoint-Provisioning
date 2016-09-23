using System;
using System.Collections.Generic;
using System.Diagnostics;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class CustomActionManager : ProvisioningManagerBase
    {
        private readonly Web _web;
        private ClientContext _ctx;

        public CustomActionManager()
        {
        }

        public CustomActionManager(ClientContext ctx) : this(ctx, ctx.Web)
        {
        }

        public CustomActionManager(ClientContext ctx, Web web)
        {
            _ctx = ctx;
            _web = web;
            _web.EnsureProperties(w => w.Title, w => w.ServerRelativeUrl, w => w.AppInstanceId, w => w.Url);
        }

        public virtual Dictionary<string, CustomActionCreator> CustomActions { get; set; }

        public void CreateAll()
        {
            if (_ctx == null)
            {
                throw new InvalidOperationException("No ClientContext");
            }
            CreateAll(_ctx, _ctx.Web);
        }

        public void CreateAll(ClientContext ctx)
        {
            _ctx = ctx;
            CreateAll(_ctx, _ctx.Web);
        }

        public void CreateAll(ClientContext ctx, Web web)
        {
            if (CustomActions != null && CustomActions.Count > 0)
            {
                try
                {
                    if (!web.IsPropertyAvailable("Title") || !web.IsPropertyAvailable("ServerRelativeUrl"))
                    {
                        ctx.Load(web, w => w.Title, w => w.ServerRelativeUrl);
                        ctx.ExecuteQueryRetry();
                    }

                    var webUserCustomActionReplacementTokens = new Dictionary<string, string>
                    {
                        {"Title", web.Title},
                        {"WebServerRelativeUrl", web.ServerRelativeUrl != "/" ? web.ServerRelativeUrl : ""},
                        {"Ticks", DateTime.Now.Ticks.ToString()}
                    };

                    DeleteAll();
                    foreach (var userCustomActionCreator in CustomActions.Values)
                    {
                        if (userCustomActionCreator.Location == "ScriptLink" &&
                            string.IsNullOrEmpty(userCustomActionCreator.ScriptBlock) &&
                            string.IsNullOrEmpty(userCustomActionCreator.ScriptSrc)) continue;

                        UserCustomAction newUserCustomAction;

                        //Can't set site collection custom actions in an app web 
                        //which is fine because app webs don't have subsites anyway!
                        if (userCustomActionCreator.SiteScope && _web.AppInstanceId == default(Guid))
                        {
                            newUserCustomAction = ctx.Site.UserCustomActions.Add();
                        }
                        else
                        {
                            newUserCustomAction = web.UserCustomActions.Add();
                        }

                        newUserCustomAction.Title = DoTokenReplacement(userCustomActionCreator.Title,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Description = userCustomActionCreator.Description;
                        newUserCustomAction.Group = userCustomActionCreator.Group;
                        newUserCustomAction.ImageUrl = DoTokenReplacement(userCustomActionCreator.ImageUrl,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Location = userCustomActionCreator.Location;
                        newUserCustomAction.RegistrationId = userCustomActionCreator.RegistrationId;
                        newUserCustomAction.RegistrationType = userCustomActionCreator.RegistrationType;
                        newUserCustomAction.ScriptBlock = DoTokenReplacement(userCustomActionCreator.ScriptBlock,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.ScriptSrc = DoTokenReplacement(userCustomActionCreator.ScriptSrc,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Sequence = userCustomActionCreator.Sequence;
                        newUserCustomAction.Url = DoTokenReplacement(userCustomActionCreator.Url,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Update();
                    }

                    ctx.ExecuteQueryRetry();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error creating user custom action " + _web.Url + " | " + ex);
                    throw;
                }
            }
        }

        public void CreateAll(ClientContext ctx, List list)
        {
            if (CustomActions != null && CustomActions.Count > 0)
            {
                try
                {
                    if (!list.IsPropertyAvailable("Title") || !list.IsPropertyAvailable("RootFolder"))
                    {
                        ctx.Load(list, l => l.Title, l => l.RootFolder);
                        ctx.ExecuteQueryRetry();
                    }

                    var listUserCustomActionReplacementTokens = new Dictionary<string, string>
                    {
                        {"Title", list.Title},
                        {"RootFolderUrl", list.RootFolder.ServerRelativeUrl}
                    };

                    DeleteAll();
                    foreach (var userCustomActionCreator in CustomActions.Values)
                    {
                        var newUserCustomAction = list.UserCustomActions.Add();

                        newUserCustomAction.Title = DoTokenReplacement(userCustomActionCreator.Title,
                            listUserCustomActionReplacementTokens);
                        newUserCustomAction.Description = userCustomActionCreator.Description;
                        newUserCustomAction.Group = userCustomActionCreator.Group;
                        newUserCustomAction.ImageUrl = DoTokenReplacement(userCustomActionCreator.ImageUrl,
                            listUserCustomActionReplacementTokens);
                        newUserCustomAction.Location = userCustomActionCreator.Location;
                        newUserCustomAction.RegistrationId = list.Id.ToString();
                        newUserCustomAction.RegistrationType = userCustomActionCreator.RegistrationType;
                        newUserCustomAction.ScriptBlock = DoTokenReplacement(userCustomActionCreator.ScriptBlock,
                            listUserCustomActionReplacementTokens);
                        newUserCustomAction.ScriptSrc = DoTokenReplacement(userCustomActionCreator.ScriptSrc,
                            listUserCustomActionReplacementTokens);
                        newUserCustomAction.Sequence = userCustomActionCreator.Sequence;
                        newUserCustomAction.Url = DoTokenReplacement(userCustomActionCreator.Url,
                            listUserCustomActionReplacementTokens);
                        newUserCustomAction.Update();
                    }

                    ctx.ExecuteQueryRetry();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error creating user custom action " + _web.Url + " | " + ex);
                    throw;
                }
            }
        }

        private string DoTokenReplacement(string tokenizedString,
            Dictionary<string, string> webUserCustomActionReplacementTokens)
        {
            if (string.IsNullOrEmpty(tokenizedString)) return string.Empty;

            var newString = tokenizedString;
            foreach (var token in webUserCustomActionReplacementTokens)
            {
                var tokenString = $"{{@{token.Key}";
                tokenString = tokenString + newString.GetInnerText(tokenString, "}") + "}";
                newString = newString.Replace(tokenString, token.Value);
            }

            return newString;
        }

        public void CreateAll(string clientId, string version)
        {
            var webUserCustomActionReplacementTokens = new Dictionary<string, string>
                    {
                        {"Title", _web.Title},
                        {"WebServerRelativeUrl", _web.ServerRelativeUrl  != "/" ? _web.ServerRelativeUrl : ""},
                        {"Ticks", DateTime.Now.Ticks.ToString()},
                        {"clientId", clientId },
                        {"version", version },
                    };

            if (CustomActions != null && CustomActions.Count > 0)
            {
                try
                {
                    DeleteAll();
                    foreach (var customActionCreator in CustomActions.Values)
                    {
                        if (customActionCreator.Location == "ScriptLink" &&
                            string.IsNullOrEmpty(customActionCreator.ScriptBlock) &&
                            string.IsNullOrEmpty(customActionCreator.ScriptSrc)) continue;

                        customActionCreator.ClientId = clientId;
                        customActionCreator.Version = version;

                        UserCustomAction newUserCustomAction;

                        //Can't set site collection custom actions in an app web 
                        //which is fine because app webs don't have subsites anyway!
                        if (customActionCreator.SiteScope && _web.AppInstanceId == default(Guid))
                        {
                            newUserCustomAction = _ctx.Site.UserCustomActions.Add();
                        }
                        else
                        {
                            newUserCustomAction = _web.UserCustomActions.Add();
                        }

                        newUserCustomAction.Title = DoTokenReplacement(customActionCreator.Title,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Description = customActionCreator.Description;
                        newUserCustomAction.Group = customActionCreator.Group;
                        newUserCustomAction.ImageUrl = DoTokenReplacement(customActionCreator.ImageUrl,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Location = customActionCreator.Location;
                        newUserCustomAction.RegistrationId = customActionCreator.RegistrationId;
                        newUserCustomAction.RegistrationType = customActionCreator.RegistrationType;
                        newUserCustomAction.ScriptBlock = DoTokenReplacement(customActionCreator.ScriptBlock,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.ScriptSrc = DoTokenReplacement(customActionCreator.ScriptSrc,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Sequence = customActionCreator.Sequence;
                        newUserCustomAction.Url = DoTokenReplacement(customActionCreator.Url,
                            webUserCustomActionReplacementTokens);
                        newUserCustomAction.Update();
                        OnNotify(ProvisioningNotificationLevels.Verbose,
                            "Adding custom action " + customActionCreator.Title);
                    }

                    _ctx.ExecuteQueryRetry();
                }
                catch (Exception ex)
                {
                    OnNotify(ProvisioningNotificationLevels.Normal,
                        "Error creating custom actions " + _web.Url + " | " + ex);
                    Trace.TraceError("Error creating custom actions " + _ctx.Web.Url + " | " + ex);
                    throw;
                }
            }
        }

        public virtual void DeleteAll()
        {
            if (CustomActions == null || CustomActions.Count == 0) return;

            var webCustomActions = _web.UserCustomActions;
            var siteCustomActions = _ctx.Site.UserCustomActions;
            _ctx.Load(webCustomActions);
            _ctx.Load(siteCustomActions);
            _ctx.ExecuteQueryRetry();

            for (var i = webCustomActions.Count - 1; i >= 0; i--)
            {
                if (webCustomActions[i].Title != null && CustomActions.ContainsKey(webCustomActions[i].Title))
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose,
                        "Removing web custom action " + webCustomActions[i].Title);
                    webCustomActions[i].DeleteObject();
                }
                //Delete the old loader if present
                else if (string.IsNullOrEmpty(webCustomActions[i].Title) &&
                         !string.IsNullOrEmpty(webCustomActions[i].ScriptBlock) &&
                         webCustomActions[i].ScriptBlock.Contains("$LAB.script"))
                {
                    webCustomActions[i].DeleteObject();
                }
            }

            for (var i = siteCustomActions.Count - 1; i >= 0; i--)
            {
                if (siteCustomActions[i].Title != null && CustomActions.ContainsKey(siteCustomActions[i].Title))
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose,
                        "Removing site custom action " + siteCustomActions[i].Title);
                    siteCustomActions[i].DeleteObject();
                }
            }

            _ctx.ExecuteQueryRetry();
        }
    }
}