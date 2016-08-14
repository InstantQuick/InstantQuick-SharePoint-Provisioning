using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    /// <summary>
    /// Builds creators for UserCustomActions scoped to Site, Web, or List
    /// </summary>
    public class CustomActionCreatorBuilder : CreatorBuilderBase
    {
        /// <summary>
        /// Returns a single custom action creator as a JSON string
        /// </summary>
        /// <param name="ctx">The client context</param>
        /// <param name="web">The web that contains the UCA</param>
        /// <param name="customActionName">The name of the UCA</param>
        /// <param name="siteScope">If true, looks for site scoped UCA's otherwise Web scope</param>
        /// <returns></returns>
        public string GetCustomActionCreator(ClientContext ctx, Web web, string customActionName, bool siteScope)
        {
            var manifest = new AppManifestBase();
            GetCustomActionCreator(ctx, web, customActionName, manifest, siteScope);
            if (manifest.CustomActionCreators.ContainsKey(customActionName))
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.CustomActionCreators[customActionName]);
            }
            OnVerboseNotify("NO INFORMATION FOUND FOR " + customActionName);
            return string.Empty;
        }

        /// <summary>
        /// Adds a single custom action creator to a given manifest
        /// </summary>
        /// <param name="ctx">The client context</param>
        /// <param name="web">The web that contains the UCA</param>
        /// <param name="customActionName">The name of the UCA</param>
        /// <param name="manifest">The manifest to which the creator is to be added</param>
        /// <param name="siteScope">If true, looks for site scoped UCA's otherwise Web scope</param>
        /// <returns></returns>
        public void GetCustomActionCreator(ClientContext ctx, Web web, string customActionName, AppManifestBase manifest,
            bool siteScope)
        {
            if (manifest == null) return;

            var existingCustomActions = manifest.CustomActionCreators;
            existingCustomActions = existingCustomActions ?? new Dictionary<string, CustomActionCreator>();
            
            CustomActionCreator creator;

            if (siteScope)
            {
                creator = GetCustomActionCreatorFromSite(ctx, customActionName);
                creator.SiteScope = true;
            }
            else
                creator = GetCustomActionCreatorFromWeb(ctx, web, customActionName);

            if (creator != null)
            {
                existingCustomActions[customActionName] = creator;
            }
            manifest.CustomActionCreators = existingCustomActions;
        }

        /// <summary>
        /// Reads a single custom action from a list by title and returns it as a JSON
        /// </summary>
        /// <param name="ctx">Client Context</param>
        /// <param name="list">List</param>
        /// <param name="customActionName">Name of the UserCustomAction to read</param>
        /// <returns></returns>
        public string GetCustomActionCreator(ClientContext ctx, List list, string customActionName)
        {
            var manifest = new AppManifestBase();
            GetCustomActionCreator(ctx, list, customActionName, manifest);
            if (manifest.CustomActionCreators.ContainsKey(customActionName))
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.CustomActionCreators[customActionName]);
            }
            OnVerboseNotify("NO INFORMATION FOUND FOR " + customActionName);
            return string.Empty;
        }

        public void GetCustomActionCreator(ClientContext ctx, List list, string customActionName,
            AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingCustomActions = manifest.CustomActionCreators;
            existingCustomActions = existingCustomActions ?? new Dictionary<string, CustomActionCreator>();


            var creator = GetCustomActionCreatorFromList(ctx, list, customActionName);
            if (creator != null)
            {
                existingCustomActions[customActionName] = creator;
            }
            manifest.CustomActionCreators = existingCustomActions;
        }

        public string GetCustomActionCreators(ClientContext ctx, Web web, bool siteScope)
        {
            var manifest = new AppManifestBase();
            GetCustomActionCreators(ctx, web, manifest, siteScope);
            if (manifest.CustomActionCreators != null)
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.CustomActionCreators);
            }
            OnVerboseNotify("No custom actions found");
            return string.Empty;
        }

        public void GetCustomActionCreators(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            if (manifest == null) return;

            var existingCustomActions = manifest.CustomActionCreators;
            existingCustomActions = existingCustomActions ?? new Dictionary<string, CustomActionCreator>();

            GetCustomActionCreatorsFromWeb(ctx, web, existingCustomActions);

            manifest.CustomActionCreators = existingCustomActions;
        }

        public void GetCustomActionCreators(ClientContext ctx, Web web, AppManifestBase manifest, bool siteScope)
        {
            if (manifest == null) return;

            var existingCustomActions = manifest.CustomActionCreators;
            existingCustomActions = existingCustomActions ?? new Dictionary<string, CustomActionCreator>();

            if (siteScope)
                GetCustomActionCreatorsFromSite(ctx, existingCustomActions);
            else
                GetCustomActionCreatorsFromWeb(ctx, web, existingCustomActions);

            manifest.CustomActionCreators = existingCustomActions;
        }

        private void GetCustomActionCreatorsFromSite(ClientContext ctx,
            Dictionary<string, CustomActionCreator> existingCustomActions)
        {
            var userCustomActions = ctx.Site.UserCustomActions;
            ctx.Load(userCustomActions,
                ucas =>
                    ucas.Include(uca => uca.Title, uca => uca.Description, uca => uca.Group, uca => uca.ImageUrl,
                        uca => uca.Location, uca => uca.RegistrationId, uca => uca.RegistrationType,
                        uca => uca.ScriptBlock, uca => uca.ScriptSrc, uca => uca.Sequence, uca => uca.Url,
                        uca => uca.CommandUIExtension));
            ctx.ExecuteQueryRetry();

            foreach (var userCustomAction in userCustomActions)
            {
                var id = userCustomAction.Title;
                if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();
                existingCustomActions[id] = GetCreatorFromUserCustomAction(userCustomAction);
                existingCustomActions[id].SiteScope = true;
            }
        }

        private void GetCustomActionCreatorsFromWeb(ClientContext ctx, Web web,
            Dictionary<string, CustomActionCreator> existingCustomActions)
        {
            var userCustomActions = web.UserCustomActions;
            ctx.Load(userCustomActions,
                ucas =>
                    ucas.Include(uca => uca.Title, uca => uca.Description, uca => uca.Group, uca => uca.ImageUrl,
                        uca => uca.Location, uca => uca.RegistrationId, uca => uca.RegistrationType,
                        uca => uca.ScriptBlock, uca => uca.ScriptSrc, uca => uca.Sequence, uca => uca.Url,
                        uca => uca.CommandUIExtension));
            ctx.ExecuteQueryRetry();

            foreach (var userCustomAction in userCustomActions)
            {
                var id = userCustomAction.Title;
                if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();
                existingCustomActions[id] = GetCreatorFromUserCustomAction(userCustomAction);
            }
        }

        public string GetCustomActionCreators(ClientContext ctx, List list, string listTitle)
        {
            var manifest = new AppManifestBase
            {
                ListCreators = new Dictionary<string, ListCreator> {[listTitle] = new ListCreator()}
            };
            GetCustomActionCreators(ctx, list, listTitle, manifest);
            var js = new JavaScriptSerializer();
            return js.Serialize(manifest.ListCreators[listTitle].CustomActionCreators);
        }

        public void GetCustomActionCreators(ClientContext ctx, List list, string listTitle, AppManifestBase manifest)
        {
            if (manifest?.ListCreators == null || !manifest.ListCreators.ContainsKey(listTitle) || list == null ||
                string.IsNullOrEmpty(listTitle)) return;

            var existingCustomActions = manifest.ListCreators[listTitle].CustomActionCreators;
            existingCustomActions = existingCustomActions ?? new Dictionary<string, CustomActionCreator>();

            GetCustomActionCreatorsFromList(ctx, list, existingCustomActions);
            manifest.ListCreators[listTitle].CustomActionCreators = existingCustomActions;
        }

        private void GetCustomActionCreatorsFromList(ClientContext ctx, List list,
            Dictionary<string, CustomActionCreator> existingCustomActions)
        {
            var userCustomActions = list.UserCustomActions;
            ctx.Load(userCustomActions,
                ucas =>
                    ucas.Include(uca => uca.Title, uca => uca.Description, uca => uca.Group, uca => uca.ImageUrl,
                        uca => uca.Location, uca => uca.RegistrationId, uca => uca.RegistrationType,
                        uca => uca.ScriptBlock, uca => uca.ScriptSrc, uca => uca.Sequence, uca => uca.Url,
                        uca => uca.CommandUIExtension));
            ctx.ExecuteQueryRetry();
            foreach (var userCustomAction in userCustomActions)
            {
                var id = userCustomAction.Title;
                if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();
                existingCustomActions[id] = GetCreatorFromUserCustomAction(userCustomAction);
            }
        }

        private CustomActionCreator GetCustomActionCreatorFromList(ClientContext ctx, List list, string customActionName)
        {
            var userCustomActions = list.UserCustomActions;
            return GetCustomActionCreator(ctx, userCustomActions, customActionName);
        }

        private CustomActionCreator GetCustomActionCreatorFromWeb(ClientContext ctx, Web web, string customActionName)
        {
            var userCustomActions = web.UserCustomActions;
            return GetCustomActionCreator(ctx, userCustomActions, customActionName);
        }

        private CustomActionCreator GetCustomActionCreatorFromSite(ClientContext ctx, string customActionName)
        {
            var userCustomActions = ctx.Site.UserCustomActions;
            return GetCustomActionCreator(ctx, userCustomActions, customActionName);
        }

        private CustomActionCreator GetCustomActionCreator(ClientContext ctx,
            UserCustomActionCollection userCustomActions, string customActionName)
        {
            ctx.Load(userCustomActions, uca => uca.Where(ca => ca.Title == customActionName));
            ctx.ExecuteQueryRetry();

            if (userCustomActions.Count == 0) return null;

            var userCustomAction = userCustomActions[0];

            var newCreator = GetCreatorFromUserCustomAction(userCustomAction);
            return newCreator;
        }

        private CustomActionCreator GetCreatorFromUserCustomAction(UserCustomAction userCustomAction)
        {
            var newCreator = new CustomActionCreator
            {
                Title = userCustomAction.Title,
                Description = userCustomAction.Description,
                Group = userCustomAction.Group,
                ImageUrl = userCustomAction.ImageUrl,
                Location = userCustomAction.Location,
                RegistrationId = userCustomAction.RegistrationId,
                RegistrationType = userCustomAction.RegistrationType,
                ScriptBlock = userCustomAction.ScriptBlock,
                ScriptSrc = userCustomAction.ScriptSrc,
                Sequence = userCustomAction.Sequence,
                Url = userCustomAction.Url,
                CommandUIExtension = userCustomAction.CommandUIExtension
            };
            return newCreator;
        }
    }
}