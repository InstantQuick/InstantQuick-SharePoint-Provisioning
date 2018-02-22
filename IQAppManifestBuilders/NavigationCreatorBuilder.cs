using System.Collections.Generic;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    //TODO: Clean up and consolidate redundant token substitution code
    public class NavigationCreatorBuilder : CreatorBuilderBase
    {
        public string GetNavigationCreator(ClientContext ctx, Web web, string navigationCollection)
        {
            var manifest = new AppManifestBase();
            GetNavigationCreator(ctx, web, navigationCollection, manifest);
            var collection = navigationCollection == "Left"
                ? manifest.Navigation.LeftNavigationNodes
                : manifest.Navigation.TopNavigationNodes;
            var js = new JavaScriptSerializer();
            return js.Serialize(collection);
        }

        public void GetNavigationCreator(ClientContext ctx, Web web, string navigationCollection,
            AppManifestBase manifest)
        {
            if (manifest == null) return;
            manifest.Navigation = manifest.Navigation ?? new NavigationCreator();

            manifest.Navigation.ClearTopMenu = manifest.Navigation.ClearTopMenu || (navigationCollection != "Left");
            manifest.Navigation.ClearLeftMenu = manifest.Navigation.ClearLeftMenu || (navigationCollection == "Left");

            var newNodeCollection = GetNavigationFromSite(ctx, web, navigationCollection);

            if (navigationCollection == "Left")
                manifest.Navigation.LeftNavigationNodes = newNodeCollection;
            else
                manifest.Navigation.TopNavigationNodes = newNodeCollection;
        }

        private Dictionary<string, NavigationNodeCreator> GetNavigationFromSite(ClientContext ctx, Web web,
            string navigationCollection)
        {
            OnVerboseNotify("Getting " + (navigationCollection == "Left" ? " left " : "top") + " for " +
                            web.ServerRelativeUrl);

            var retVal = new Dictionary<string, NavigationNodeCreator>();

            var rootWeb = ctx.Site.RootWeb;
            ctx.Load(rootWeb, w => w.ServerRelativeUrl, w => w.Url);

            var navigationNodeCollection = navigationCollection == "Left"
                ? web.Navigation.QuickLaunch
                : web.Navigation.TopNavigationBar;
            ctx.Load(navigationNodeCollection,
                nvc => nvc.Include(node => node.IsExternal, node => node.Title, node => node.Url,
                    node => node.Children.Include(child => child.IsExternal, child => child.Title, child => child.Url)));

            ctx.ExecuteQueryRetry();

            foreach (var node in navigationNodeCollection)
            {
                var newCreatorNode = new NavigationNodeCreator
                {
                    AsLastNode = true,
                    IsExternal = node.IsExternal,
                    Title = node.Title
                };

                newCreatorNode.Url = node.Url.Replace(web.Url, "{@WebUrl}");
                if (ctx.Web.ServerRelativeUrl != "/")
                    newCreatorNode.Url = node.Url.Replace(web.ServerRelativeUrl, "{@WebServerRelativeUrl}");

                if (web.Url != rootWeb.Url)
                {
                    newCreatorNode.Url = node.Url.Replace(rootWeb.Url, "{@SiteUrl}");
                    newCreatorNode.Url = node.Url.Replace(rootWeb.ServerRelativeUrl, "{@SiteServerRelativeUrl}");
                }

                if (node.Children != null && node.Children.Count > 0)
                {
                    //TODO: Consider a recursive function instead of only allowing one level deep
                    newCreatorNode.Children = new List<NavigationNodeCreator>();
                    foreach (var child in node.Children)
                    {
                        var newChildNode = new NavigationNodeCreator
                        {
                            AsLastNode = true,
                            IsExternal = child.IsExternal,
                            Title = child.Title
                        };
                        newChildNode.Url = child.Url.Replace(web.Url, "{@WebUrl}");
                        if (ctx.Web.ServerRelativeUrl != "/")
                            newChildNode.Url = child.Url.Replace(web.ServerRelativeUrl, "{@WebServerRelativeUrl}");

                        if (web.Url != rootWeb.Url)
                        {
                            newChildNode.Url = child.Url.Replace(rootWeb.Url, "{@SiteUrl}");
                            newChildNode.Url = child.Url.Replace(rootWeb.ServerRelativeUrl, "{@SiteServerRelativeUrl}");
                        }

                        newCreatorNode.Children.Add(newChildNode);
                    }
                }
                retVal.Add(newCreatorNode.Title, newCreatorNode);
            }
            return retVal;
        }
    }
}