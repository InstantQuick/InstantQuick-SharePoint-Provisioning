using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class NavigationManager : ProvisioningManagerBase
    {
        private readonly ClientContext _ctx;
        private readonly Web _web;

        public NavigationManager(ClientContext ctx) : this(ctx, ctx.Web)
        {
        }

        public NavigationManager(ClientContext ctx, Web web)
        {
            _ctx = ctx;
            _web = web;
            _ctx.Load(_web, w => w.ServerRelativeUrl, w => w.AppInstanceId);
            _ctx.Load(_web.Navigation, n => n.QuickLaunch, n => n.TopNavigationBar);
            _ctx.ExecuteQueryRetry();
        }

        public virtual bool ClearTopMenu { get; set; }
        public virtual bool ClearLeftMenu { get; set; }
        public virtual Dictionary<string, NavigationNodeCreator> TopNavigationNodes { get; set; }
        public virtual Dictionary<string, NavigationNodeCreator> LeftNavigationNodes { get; set; }

        public virtual void Provision()
        {
            var navigation = _web.Navigation;
            if (ClearLeftMenu)
            {
                for (var i = navigation.QuickLaunch.Count - 1; i >= 0; i--)
                {
                    navigation.QuickLaunch[i].DeleteObject();
                }
                _ctx.ExecuteQueryRetry();
            }
            if (ClearTopMenu)
            {
                for (var i = navigation.TopNavigationBar.Count - 1; i >= 0; i--)
                {
                    navigation.TopNavigationBar[i].DeleteObject();
                }
                _ctx.ExecuteQueryRetry();
            }
            if (TopNavigationNodes != null)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, "Adding top navigation nodes");
                try
                {
                    _web.Navigation.UseShared = false;
                    _web.Update();
                    _ctx.ExecuteQueryRetry();
                }
                catch
                {
                    // ignored
                }

                foreach (var node in TopNavigationNodes.Values)
                {
                    AddNode(node, navigation.TopNavigationBar);
                }
            }
            if (LeftNavigationNodes != null)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, "Adding left navigation nodes");
                foreach (var node in LeftNavigationNodes.Values)
                {
                    AddNode(node, navigation.QuickLaunch);
                }
            }
            _ctx.ExecuteQueryRetry();
        }

        private void AddNode(NavigationNodeCreator node, NavigationNodeCollection navigationNodes)
        {
            var found = false;
            foreach (var existingNode in navigationNodes)
            {
                if (node.Title == existingNode.Title)
                {
                    found = true;
                    if (node.Children != null && node.Children.Count > 0)
                    {
                        _ctx.Load(existingNode, n => n.Children.Include(c => c.Title));
                        _ctx.ExecuteQueryRetry();
                        foreach (var childNode in node.Children)
                        {
                            AddNode(childNode, existingNode.Children);
                        }
                    }
                    break;
                }
            }
            if (!found)
            {
                var creator = new NavigationNodeCreationInformation
                {
                    Title = node.Title,
                    IsExternal = node.IsExternal,
                    Url =
                        node.IsExternal
                            ? node.Url.Replace("{@WebUrl}", _web.Url).Replace("{@WebServerRelativeUrl}", _web.ServerRelativeUrl)
                            : GetUrl(node.Url)
                };
                if (node.AsLastNode)
                {
                    creator.AsLastNode = true;
                }
                var newNode = navigationNodes.Add(creator);

                if (node.Children != null && node.Children.Count > 0)
                {
                    _ctx.Load(newNode, n => n.Children.Include(c => c.Title));
                    _ctx.ExecuteQueryRetry();
                    foreach (var childNode in node.Children)
                    {
                        AddNode(childNode, newNode.Children);
                    }
                }
            }
        }

        private string GetUrl(string url)
        {
            var retVal = url;
            if (retVal.Contains("{@WebUrl}") || retVal.Contains("{@WebServerRelativeUrl}"))
            {
                retVal = retVal.Replace("{@WebUrl}", _web.Url).Replace("{@WebServerRelativeUrl}", _web.ServerRelativeUrl);
            }
            else if (_web.ServerRelativeUrl != "/") retVal = _web.ServerRelativeUrl + url;
            return retVal;
        }

        public virtual void UnProvision()
        {
            OnNotify(ProvisioningNotificationLevels.Verbose, "Removing navigation nodes");
            var delList = new List<NavigationNode>();

            var top = _web.Navigation.TopNavigationBar;
            var left = _web.Navigation.QuickLaunch;
            foreach (var creator in TopNavigationNodes.Values)
            {
                foreach (var node in top)
                {
                    if (creator.Title == node.Title)
                    {
                        delList.Add(node);
                        break;
                    }
                }
            }
            foreach (var creator in LeftNavigationNodes.Values)
            {
                foreach (var node in left)
                {
                    if (creator.Title == node.Title)
                    {
                        delList.Add(node);
                        break;
                    }
                }
            }
            foreach (var node in delList)
            {
                node.DeleteObject();
            }

            _ctx.ExecuteQueryRetry();
        }

        public CustomActionCreator CreateNavigationUserCustomAction(NavigationCreator navigation)
        {
            OnNotify(ProvisioningNotificationLevels.Verbose, "Creating navigation custom action for app web");
            var creator = new CustomActionCreator
            {
                Location = "ScriptLink",
                Sequence = 9999,
                Title = "IQAppWebNavigation",
                RegistrationType = UserCustomActionRegistrationType.None
            };

            var js = new JavaScriptSerializer();
            var navJson = js.Serialize(navigation);

            var customActionTemplate =
                Utility.GetFile("IQAppProvisioningBaseClasses.Resources.AppWebNavigationCustomAction.min.js",
                    Assembly.GetExecutingAssembly());
            creator.ScriptBlock =
                customActionTemplate.Replace("{@NavigationJSON}", navJson)
                    .Replace("{@WebServerRelativeUrl}", _web.ServerRelativeUrl);

            return creator;
        }
    }
}