#region Imports (8)

using System;
using System.Collections.Generic;
using System.Text;
using IQAppProvisioningBaseClasses.Events;
using IQAppRuntimeResources;
using Microsoft.SharePoint.Client;

#endregion Imports (8)

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class RuntimeResourceMappingsManager : ProvisioningManagerBase
    {
        private readonly ClientContext _ctx;
        private readonly string _appSettingsCustomActionWrapper = "(function(){{ var n = window.IQApp = window.IQApp || {{}};"
                    + "var g = n.globalResources = n.globalResources || {{}};"
                    + "g.scripts = g.scripts || [];"
                    + "g.styles = g.styles || []; {0} }})();";
        private readonly string _appSettingsJsWrapper = "(function () {{"
                    + "window.IQApp = window.IQApp || {{}};"
                    + "var settings = window.IQApp;"
                    + "if (settings.disableApp || settings.urlParams.disableApp !== undefined) return;"
                    + "if (settings.urlParams.VisibilityContext === 'WSSWebPartPage') return;"
                    + "settings.pages = settings.pages || {{}};"
                    + "settings.contentTypeActions = settings.contentTypeActions || {{}};"
                    + "{0};"
                    + "{1};"
                    + "}})();";
        private readonly string _globalScript = "g.scripts.push({{ url:'{0}', wait:{1}, external:{2} }});";
        private readonly string _globalStyle = "g.styles.push({{ url:'{0}', external:{1} }});";
        private readonly string _pageResource = "settings.pages[\"{0}\"] = {{ scripts: [ {1} ], styles: [ {2} ] }};";
        private readonly string _pageScript = "{{url:{0}, wait:{1} }}";

        public RuntimeResourceMappingsManager(ClientContext ctx)
        {
            _ctx = ctx;
        }

        private void CreateGlobalResourcesCustomAction(string manifestName, string globalResources)
        {
            var customActionName = manifestName + " Generated Mappings";
            var customActionManager = new CustomActionManager(_ctx);
            var customActions = new Dictionary<string, CustomActionCreator>
            {
                [customActionName] = new CustomActionCreator
                {
                    SiteScope = false,
                    Location = "ScriptLink",
                    Sequence = _sequence + 100,
                    ScriptBlock = string.Format(_appSettingsCustomActionWrapper, globalResources),
                    Title = customActionName
                }
            };
            customActionManager.CustomActions = customActions;
            customActionManager.CreateAll(_ctx, _ctx.Web);
            OnNotify(ProvisioningNotificationLevels.Verbose, "Provisioned generated custom action " + customActionName);
        }

        private void DeleteGlobalResourcesCustomAction(string manifestName, string globalResources)
        {
            var customActionName = manifestName + " Generated Mappings";
            var customActionManager = new CustomActionManager(_ctx);
            var customActions = new Dictionary<string, CustomActionCreator>
            {
                [customActionName] = new CustomActionCreator
                {
                    SiteScope = false,
                    Location = "ScriptLink",
                    Sequence = _sequence,
                    ScriptBlock = string.Format(_appSettingsCustomActionWrapper, globalResources),
                    Title = customActionName
                }
            };
            customActionManager.CustomActions = customActions;
            customActionManager.DeleteAll();
            OnNotify(ProvisioningNotificationLevels.Verbose, "Removed generated custom action " + customActionName);
        }

        private void CreateSettingsJs(string settingsUrl, string manifestName, string appSettingsJs)
        {
            AddFolderButDontFail("/_catalogs");
            AddFolderButDontFail("/_catalogs/IQApps");
            AddFolderButDontFail("/_catalogs/IQApps/scripts");
            AddFolderButDontFail("/_catalogs/IQApps/scripts/" + manifestName);

            var fileCreationInformation = new FileCreationInformation();
            var content = Encoding.Default.GetBytes(appSettingsJs);
            fileCreationInformation.Content = content;
            fileCreationInformation.Overwrite = true;
            if (_ctx.Web.ServerRelativeUrl != "/") settingsUrl = _ctx.Web.ServerRelativeUrl + settingsUrl;
            fileCreationInformation.Url = settingsUrl;
            var folder = _ctx.Web.RootFolder;
            folder.Files.Add(fileCreationInformation);
            _ctx.ExecuteQueryRetry();
            OnNotify(ProvisioningNotificationLevels.Verbose, "Created app settings file " + settingsUrl);
        }

        private void AddFolderButDontFail(string folder)
        {
            try
            {
                var root = _ctx.Web.RootFolder;
                if (folder.StartsWith("/")) folder = folder.Substring(1);
                root.Folders.Add(folder);
                _ctx.ExecuteQueryRetry();
            }
            catch
            {
                // ignored
            }
            OnNotify(ProvisioningNotificationLevels.Verbose, "Ensured folder " + folder);
        }

        private string MapContentTypeActions(AppManifestBase manifest, string contentTypeActions)
        {
            if (manifest.ContentTypeCreators != null)
            {
                foreach (var creator in manifest.ContentTypeCreators)
                {
                    if (!String.IsNullOrEmpty(creator.Value.DisplayFormUrl) || !String.IsNullOrEmpty(creator.Value.EditFormUrl) || !String.IsNullOrEmpty(creator.Value.NewFormUrl) || !String.IsNullOrEmpty(creator.Value.BaseViewUrl))
                    {
                        var creatorValue = creator.Value;

                        var mapping = "settings.contentTypeActions['" + creator.Key + "']={";
                        //TODO: Fix naming differences
                        if (!String.IsNullOrEmpty(creatorValue.DisplayFormUrl))
                        {
                            string properties;
                            if (creatorValue.DisplayFormIsDialog)
                            {
                                properties = "url: settings.serverRelativeUrl + '" + creatorValue.DisplayFormUrl + "',";
                                properties += "isDialog: true," +
                                    (!string.IsNullOrEmpty(creatorValue.DisplayFormDialogTitle) ? "title: '" + creatorValue.DisplayFormDialogTitle + "'," : string.Empty) +
                                    (creatorValue.DisplayFormDialogHeight != null ? "height: " + creatorValue.DisplayFormDialogHeight + "," : string.Empty) +
                                    (creatorValue.DisplayFormDialogWidth != null ? "width: " + creatorValue.DisplayFormDialogWidth + "," : string.Empty);

                            }
                            else
                            {
                                properties = "url: settings.serverRelativeUrl + '" + creatorValue.DisplayFormUrl + "'";
                            }

                            mapping = AddToMapping(mapping, "DisplayUrl:{" + properties + "}");
                        }
                        if (!String.IsNullOrEmpty(creatorValue.EditFormUrl))
                        {
                            string properties;
                            if (creatorValue.EditFormIsDialog)
                            {
                                properties = "url: settings.serverRelativeUrl + '" + creatorValue.EditFormUrl + "',";
                                properties += "isDialog: true," +
                                    (!string.IsNullOrEmpty(creatorValue.EditFormDialogTitle) ? "title: '" + creatorValue.EditFormDialogTitle + "'," : string.Empty) +
                                    (creatorValue.EditFormDialogHeight != null ? "height: " + creatorValue.EditFormDialogHeight + "," : string.Empty) +
                                    (creatorValue.EditFormDialogWidth != null ? "width: " + creatorValue.EditFormDialogWidth + "," : string.Empty);

                            }
                            else
                            {
                                properties = "url: settings.serverRelativeUrl + '" + creatorValue.EditFormUrl + "'";
                            }

                            mapping = AddToMapping(mapping, "EditUrl:{" + properties + "}");
                        }
                        if (!String.IsNullOrEmpty(creatorValue.BaseViewUrl))
                        {
                            mapping = AddToMapping(mapping, "BaseViewUrl: settings.serverRelativeUrl + '" + creatorValue.BaseViewUrl + "'");
                        }
                        if (!String.IsNullOrEmpty(creatorValue.NewFormUrl))
                        {
                            string properties;
                            if (creatorValue.NewFormIsDialog)
                            {
                                properties = "url: settings.serverRelativeUrl + '" + creatorValue.NewFormUrl + "',";
                                properties += "isDialog: true," +
                                    (!string.IsNullOrEmpty(creatorValue.NewFormDialogTitle) ? "title: '" + creatorValue.NewFormDialogTitle + "'," : string.Empty) +
                                    (creatorValue.NewFormDialogHeight != null ? "height: " + creatorValue.NewFormDialogHeight + "," : string.Empty) +
                                    (creatorValue.NewFormDialogWidth != null ? "width: " + creatorValue.NewFormDialogWidth + "," : string.Empty);

                            }
                            else
                            {
                                properties = "url: settings.serverRelativeUrl + '" + creatorValue.NewFormUrl + "'";
                            }

                            mapping = AddToMapping(mapping, "NewUrl:{" + properties + "}");
                        }
                        mapping += "}";
                        if (contentTypeActions == string.Empty)
                        {
                            contentTypeActions = mapping;
                        }
                        else
                        {
                            contentTypeActions += "," + mapping;
                        }
                        OnNotify(ProvisioningNotificationLevels.Verbose, "Mapped custom UI for content type: " + creator.Key);
                    }
                }
            }
            return contentTypeActions;
        }

        private string AddToMapping(string mappings, string mapping)
        {
            if (mappings.EndsWith("{"))
            {
                mappings += mapping;
            }
            else
            {
                mappings += "," + mapping;
            }
            return mappings;
        }

        private string MapGlobalResources(AppManifestBase manifest, string globalResources)
        {
            if (manifest.ClientGlobalRuntimeResources != null && manifest.ClientGlobalRuntimeResources.Count > 0)
            {
                foreach (var resource in manifest.ClientGlobalRuntimeResources)
                {
                    if (resource.Value.ResourceType == ResourceTypes.Script)
                    {
                        globalResources += string.Format(_globalScript, resource.Value.Url, resource.Value.Wait.ToString().ToLower(), (!resource.Value.PrependWebServerRelativeUrl).ToString().ToLower());
                    }
                    else
                    {
                        globalResources += string.Format(_globalStyle, resource.Value.Url, (!resource.Value.PrependWebServerRelativeUrl).ToString().ToLower());
                    }
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Mapped resource: " + resource.Key);
                }
            }
            return globalResources;
        }

        private string MapPageResources(AppManifestBase manifest, string pageResources)
        {
            if (manifest.ClientPageRuntimeResources != null && manifest.ClientPageRuntimeResources.Count > 0)
            {
                foreach (var resource in manifest.ClientPageRuntimeResources)
                {
                    var scripts = string.Empty;
                    var styles = string.Empty;
                    if (resource.Value.Scripts != null)
                    {
                        foreach (var pageScript in resource.Value.Scripts)
                        {
                            //_pageScript = "{{url:{0}, wait:{1} }}"
                            var script = string.Format(_pageScript, pageScript.PrependWebServerRelativeUrl ? "settings.serverRelativeUrl + '" + pageScript.Url + "'" : "'" + pageScript.Url + "'", pageScript.Wait.ToString().ToLower());
                            if (scripts == string.Empty)
                            {
                                scripts = script;
                            }
                            else
                            {
                                scripts += "," + script;
                            }
                        }
                    }
                    if (resource.Value.StyleSheets != null)
                    {
                        foreach (var pageStyle in resource.Value.StyleSheets)
                        {
                            var style = (pageStyle.PrependWebServerRelativeUrl ? "settings.serverRelativeUrl + '" + pageStyle.Url + "'" : "'" + pageStyle.Url + "'");
                            if (styles == string.Empty)
                            {
                                styles = style;
                            }
                            else
                            {
                                styles += "," + style;
                            }
                        }
                    }
                    //private string _pageResource = "pages[\"{0}\"] = {{ scripts: [ {1} ], styles: [ {2} ] }}";
                    var pageResource = string.Format(_pageResource, resource.Key, scripts, styles);
                    pageResources += pageResource;
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Mapped resource: " + resource.Key);
                }
            }
            return pageResources;
        }

        private int _sequence = 1;

        public void Provision(AppManifestBase manifest, int sequence)
        {
            _sequence = sequence;
            var hasResources = true;
            var hasCustomListForms = false;
            if ((manifest.ClientGlobalRuntimeResources == null || manifest.ClientGlobalRuntimeResources.Count == 0) && (manifest.ClientPageRuntimeResources == null || manifest.ClientPageRuntimeResources.Count == 0))
            {
                hasResources = false;
            }

            if (manifest.ContentTypeCreators != null && manifest.ContentTypeCreators.Count > 0)
            {
                foreach (var creator in manifest.ContentTypeCreators.Values)
                {
                    if (!String.IsNullOrEmpty(creator.DisplayFormUrl) || !String.IsNullOrEmpty(creator.EditFormUrl) || !String.IsNullOrEmpty(creator.NewFormUrl))
                    {
                        hasCustomListForms = true;
                        break;
                    }
                }
            }

            if (!hasResources && !hasCustomListForms)
            {
                OnNotify(ProvisioningNotificationLevels.Verbose, "No resources or custom list forms found. Skipping.");
                return;
            }

            var globalResources = string.Empty;
            var pageResources = string.Empty;
            var contentTypeActions = string.Empty;

            globalResources = MapGlobalResources(manifest, globalResources);
            pageResources = MapPageResources(manifest, pageResources);
            contentTypeActions = MapContentTypeActions(manifest, contentTypeActions);

            if (pageResources != string.Empty || contentTypeActions != string.Empty)
            {
                var settingsUrl = "/_catalogs/IQApps/scripts/" + manifest.ManifestName + "/settings.js";
                var appSettingsJs = string.Format(_appSettingsJsWrapper, pageResources, contentTypeActions);
                var appSettings = string.Format(_globalScript, settingsUrl, "true", "false");
                globalResources = appSettings + globalResources;
                CreateSettingsJs(settingsUrl, manifest.ManifestName, appSettingsJs);
            }
            if (globalResources != string.Empty)
            {
                CreateGlobalResourcesCustomAction(manifest.ManifestName, globalResources);
            }
        }
        public void DeleteGlobalResources(AppManifestBase manifest)
        {
            var globalResources = string.Empty;
            globalResources = MapGlobalResources(manifest, globalResources);
            DeleteGlobalResourcesCustomAction(manifest.ManifestName, globalResources);
        }
    }
}
