using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using static IQAppProvisioningBaseClasses.Utility.Tokenizer;

namespace IQAppManifestBuilders
{
    public class WebCreatorBuilderOptions
    {
        public List<string> IncludeFilesFromTheseLibraries { get; set; } = new List<string>();
        public List<string> IncludeItemsTheseLists { get; set; } = new List<string>();
        public List<string> IncludeFilesFromTheseFolders { get; set; } = new List<string>();
        public bool IncludeNavigation { get; set; } = true;
        public bool IncludeRemoteEventReceivers { get; set; } = true;
        public bool IncludeWebCustomActions { get; set; } = true;
        public bool IncludeSiteCustomActions { get; set; } = true;
        public bool IncludeLookAndFeel { get; set; } = true;

        /// <summary>
        ///     The builder will exclude property bag entries that start with these preambles
        /// </summary>
        public List<string> PropertyBagExclusionPreambles { get; set; } = new List<string>
        {
            "DesignPreview",
            "OriginalNotebookUrl",
            "profileschemaversion",
            "vti_",
            "dlc_"
        };
    }

    public class WebCreatorBuilder : CreatorBuilderBase
    {
        private ClientContext _baseContext;
        private WebCreatorBuilderOptions _options = new WebCreatorBuilderOptions();
        private ClientContext _sourceContext;

        public void GetWebCreatorBuilder(SiteDefinition siteDefinition, ClientContext sourceContext,
            ClientContext baseContext)
        {
            if (siteDefinition == null)
            {
                throw new ArgumentException("SiteDefinition is required!");
            }
            if (siteDefinition.WebDefinition == null) siteDefinition.WebDefinition = new WebCreator();
            GetWebCreatorBuilder(siteDefinition, siteDefinition.WebDefinition, null, sourceContext, baseContext);
        }

        public void GetWebCreatorBuilder(SiteDefinition siteDefinition, WebCreator webDefinition,
            WebCreatorBuilderOptions options, ClientContext sourceContext, ClientContext baseContext)
        {
            _baseContext = baseContext;
            _sourceContext = sourceContext;
            var js = new JavaScriptSerializer();
            var tempManifestFromBase = new AppManifestBase();

            if (webDefinition == null)
            {
                throw new ArgumentException("WebDefinition is required!");
            }
            if (options != null)
            {
                _options = options;
            }
            else
            {
                _options = new WebCreatorBuilderOptions();
            }

            var web = _sourceContext.Web;
            _sourceContext.Site.EnsureProperties(p => p.RootWeb);

            var isRootWeb = web.Url == _sourceContext.Site.RootWeb.Url;
            if (isRootWeb)
            {
                webDefinition.Url = "/";
            }
            //If this isn't the root and there isn't a value already,
            //set it to the site collection relative url by striping the rootweb url.
            else if (string.IsNullOrEmpty(webDefinition.Url))
            {
                webDefinition.Url = web.ServerRelativeUrl.Substring(_sourceContext.Site.RootWeb.ServerRelativeUrl.Length);
            }

            //Ensure that the Url is valid per the structure of the site definition
            if (!ValidateUrl(siteDefinition, webDefinition))
            {
                throw new InvalidOperationException(
                    "Web definition is invalid because the url either doesn't match the collection key or because it doesn't respect the hierarchy!");
            }

            if (siteDefinition.StorageType == StorageTypes.AzureStorage)
            {
                var storageInfo = siteDefinition.GetAzureStorageInfo();
                webDefinition.AppManifest.SetAzureStorageInfo(storageInfo.Account, storageInfo.AccountKey,
                    storageInfo.Container);
            }

            webDefinition.AppManifest.BaseFilePath =
                Path.GetFullPath(siteDefinition.BaseFilePath +
                                 (webDefinition.Url != "/" ? webDefinition.Url : string.Empty));

            GetBaseWebDefinition(webDefinition, isRootWeb);

            var creatorBuilder = new CreatorBuilder();
            creatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);
            if (_options.IncludeNavigation)
            {
                OnVerboseNotify("Getting top navigation");
                creatorBuilder.GetCreator(_sourceContext, web, "Top", webDefinition.AppManifest,
                    CreatorTypes.Navigation);

                creatorBuilder.GetCreator(_baseContext, _baseContext.Web, "Top", tempManifestFromBase,
                    CreatorTypes.Navigation);

                if (js.Serialize(webDefinition.AppManifest.Navigation.TopNavigationNodes) ==
                    js.Serialize(tempManifestFromBase.Navigation.TopNavigationNodes))
                {
                    OnVerboseNotify("No differences in top nav found.");
                    webDefinition.AppManifest.Navigation.TopNavigationNodes = null;
                }
                else
                {
                    OnVerboseNotify("Differences in top nav found.");
                }

                OnVerboseNotify("Getting left navigation");

                creatorBuilder.GetCreator(_sourceContext, web, "Left", webDefinition.AppManifest,
                    CreatorTypes.Navigation);

                creatorBuilder.GetCreator(_baseContext, _baseContext.Web, "Left", tempManifestFromBase,
                    CreatorTypes.Navigation);

                if (js.Serialize(webDefinition.AppManifest.Navigation.LeftNavigationNodes) ==
                    js.Serialize(tempManifestFromBase.Navigation.LeftNavigationNodes))
                {
                    OnVerboseNotify("No differences in left nav found.");
                    webDefinition.AppManifest.Navigation.LeftNavigationNodes = null;
                }
                else
                {
                    OnVerboseNotify("Differences in left nav found.");
                }
            }
            if (_options.IncludeRemoteEventReceivers)
            {
                OnVerboseNotify("Getting remote event receivers");
                creatorBuilder.GetCreator(_sourceContext, web, string.Empty, webDefinition.AppManifest,
                    CreatorTypes.RemoteEvents);

                if (webDefinition.AppManifest.RemoteEventRegistrationCreators == null ||
                    webDefinition.AppManifest.RemoteEventRegistrationCreators.Count == 0)
                {
                    OnVerboseNotify("No remote event receivers found");
                }
                else
                {
                    creatorBuilder.GetCreator(_baseContext, _baseContext.Web, string.Empty, tempManifestFromBase,
                        CreatorTypes.RemoteEvents);

                    var matchingReceivers =
                        webDefinition.AppManifest.RemoteEventRegistrationCreators.Where(
                            s =>
                                tempManifestFromBase.RemoteEventRegistrationCreators.FirstOrDefault(
                                    b =>
                                        b.ListTitle == s.ListTitle && b.EndpointUrl == s.EndpointUrl &&
                                        b.EventReceiverType == s.EventReceiverType && b.Eventname == s.Eventname) !=
                                null).ToList();

                    if (matchingReceivers.Count != 0)
                    {
                        for (int c = matchingReceivers.Count; c >= 0; c--)
                        {
                            webDefinition.AppManifest.RemoteEventRegistrationCreators.Remove(matchingReceivers[c]);
                        }
                    }
                    if (webDefinition.AppManifest.RemoteEventRegistrationCreators.Count == 0)
                    {
                        OnVerboseNotify("No remote event receivers found");
                    }
                    else
                    {
                        OnVerboseNotify("Found remote event receivers");
                    }
                }
            }
            if (_options.IncludeLookAndFeel)
            {
                OnVerboseNotify("Getting look and feel");
                creatorBuilder.GetCreator(_sourceContext, web, string.Empty, webDefinition.AppManifest,
                    CreatorTypes.LookAndFeel);
                creatorBuilder.GetCreator(_baseContext, _baseContext.Web, string.Empty, tempManifestFromBase,
                    CreatorTypes.LookAndFeel);

                if (webDefinition.AppManifest.LookAndFeel.SiteLogoUrl == tempManifestFromBase.LookAndFeel.SiteLogoUrl)
                {
                    webDefinition.AppManifest.LookAndFeel.SiteLogoUrl = string.Empty;
                }
                else
                {
                    OnVerboseNotify($"Setting SiteLogoUrl to {webDefinition.AppManifest.LookAndFeel.SiteLogoUrl}");
                }
                if (webDefinition.AppManifest.LookAndFeel.AlternateCssUrl == tempManifestFromBase.LookAndFeel.AlternateCssUrl)
                {
                    webDefinition.AppManifest.LookAndFeel.AlternateCssUrl = string.Empty;
                }
                else
                {
                    OnVerboseNotify($"Setting AlternateCssUrl to {webDefinition.AppManifest.LookAndFeel.AlternateCssUrl}");
                }
                if (webDefinition.AppManifest.LookAndFeel.CustomMasterPageUrl == tempManifestFromBase.LookAndFeel.CustomMasterPageUrl)
                {
                    webDefinition.AppManifest.LookAndFeel.CustomMasterPageUrl = string.Empty;
                }
                else
                {
                    OnVerboseNotify($"Setting CustomMasterPageUrl to {webDefinition.AppManifest.LookAndFeel.CustomMasterPageUrl}");
                }
                if (webDefinition.AppManifest.LookAndFeel.DefaultMasterPageUrl == tempManifestFromBase.LookAndFeel.DefaultMasterPageUrl)
                {
                    webDefinition.AppManifest.LookAndFeel.DefaultMasterPageUrl = string.Empty;
                }
                else
                {
                    OnVerboseNotify($"Setting DefaultMasterPageUrl to {webDefinition.AppManifest.LookAndFeel.DefaultMasterPageUrl}");
                }

                if (js.Serialize(webDefinition.AppManifest.LookAndFeel.CurrentComposedLook) ==
                    js.Serialize(tempManifestFromBase.LookAndFeel.CurrentComposedLook))
                {
                    webDefinition.AppManifest.LookAndFeel.CurrentComposedLook = null;
                }
                else
                {
                    OnVerboseNotify("Found differences in current composed looks.");
                }

            }
            var customActionCreatorBuilder = new CustomActionCreatorBuilder();
            customActionCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);
            if (_options.IncludeSiteCustomActions || _options.IncludeWebCustomActions)
            {
                if (_options.IncludeSiteCustomActions)
                {
                    OnVerboseNotify("Getting site custom actions");
                    customActionCreatorBuilder.GetCustomActionCreators(_sourceContext, web, webDefinition.AppManifest,
                        true);
                    customActionCreatorBuilder.GetCustomActionCreators(_baseContext, _baseContext.Web,
                        tempManifestFromBase, true);
                }
                if (_options.IncludeWebCustomActions)
                {
                    OnVerboseNotify("Getting web custom actions");
                    customActionCreatorBuilder.GetCustomActionCreators(_sourceContext, web, webDefinition.AppManifest,
                        false);
                    customActionCreatorBuilder.GetCustomActionCreators(_baseContext, _baseContext.Web,
                        tempManifestFromBase, false);
                }
                var matchingCustomActions =
                    webDefinition.AppManifest.CustomActionCreators.Values.Where(
                        sca =>
                            tempManifestFromBase.CustomActionCreators.Values.FirstOrDefault(
                                bca => bca.SiteScope == sca.SiteScope && bca.Url == sca.Url
                                       && bca.CommandUIExtension == sca.CommandUIExtension &&
                                       bca.Description == sca.Description &&
                                       bca.Group == sca.Group && bca.ImageUrl == sca.ImageUrl &&
                                       bca.Location == sca.Location &&
                                       bca.RegistrationId == sca.RegistrationId &&
                                       bca.ScriptBlock == sca.ScriptBlock &&
                                       bca.ScriptSrc == sca.ScriptSrc && bca.Title == sca.Title) !=
                            null);

                foreach (var matchingCustomAction in matchingCustomActions)
                {
                    webDefinition.AppManifest.CustomActionCreators.Remove(matchingCustomAction.Title);
                }
                if (webDefinition.AppManifest.CustomActionCreators.Count > 0)
                {
                    OnVerboseNotify("Found custom actions.");
                }
            }
            if (_options.IncludeFilesFromTheseLibraries.Count > 0)
            {
                var fileCreatorBuilder = new FileCreatorBuilder();
                fileCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);
                foreach (var libraryTitle in _options.IncludeFilesFromTheseLibraries)
                {
                    var library = web.Lists.GetByTitle(libraryTitle);
                    CamlQuery query = CamlQuery.CreateAllItemsQuery(2000);

                    var fileItems = library.GetItems(query);
                    _sourceContext.Load(fileItems, fi => fi.Include(f => f.FieldValuesAsText, f => f.File));
                    _sourceContext.ExecuteQuery();

                    foreach (var fileItem in fileItems)
                    {
                        //Folders items don't have an associated file
                        if (fileItem.File.ServerObjectIsNull != null && fileItem.File.ServerObjectIsNull == false)
                        {
                            fileCreatorBuilder.GetFileCreator(_sourceContext, web,
                                fileItem.File.ServerRelativeUrl.Replace(web.ServerRelativeUrl, ""),
                                webDefinition.AppManifest.BaseFilePath, webDefinition.AppManifest, false);
                        }
                    }
                }
            }
            if (_options.IncludeFilesFromTheseFolders.Count > 0)
            {
                var baseUrl = web.ServerRelativeUrl == "/" ? string.Empty : web.ServerRelativeUrl;
                var fileCreatorBuilder = new FileCreatorBuilder();
                fileCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);
                foreach (var folder in _options.IncludeFilesFromTheseFolders)
                {
                    var searchFolder = folder;
                    if (!searchFolder.StartsWith("/")) searchFolder = "/" + folder;
                    var containerFolderUrl = baseUrl + searchFolder;
                    var spFolder = web.GetFolderByServerRelativeUrl(containerFolderUrl);
                    _sourceContext.Load(spFolder, f => f.Files.Include(file => file.Name, file => file.ServerRelativeUrl));
                    _sourceContext.ExecuteQueryRetry();
                    foreach (var file in spFolder.Files)
                    {
                        fileCreatorBuilder.GetFileCreator(_sourceContext, web,
                                file.ServerRelativeUrl.Replace(web.ServerRelativeUrl, ""),
                                webDefinition.AppManifest.BaseFilePath, webDefinition.AppManifest, false);
                    }
                }
            }
        }

        private bool ValidateUrl(SiteDefinition siteDefinition, WebCreator webDefinition)
        {
            var currentWeb = siteDefinition.WebDefinition;
            if (currentWeb == webDefinition && webDefinition.Url == "/") return true;
            if (currentWeb == webDefinition && webDefinition.Url != "/") return false;

            var urlToValidate = webDefinition.Url;
            if (urlToValidate.StartsWith("/")) urlToValidate = urlToValidate.Substring(1);
            var urlParts = urlToValidate.Split('/');

            var urlPartIndex = 0;
            var currentUrl = string.Empty;
            do
            {
                currentUrl = $"{currentUrl}/{urlParts[urlPartIndex]}".ToLowerInvariant();
                var currentWebItem = currentWeb?.Webs?.FirstOrDefault(w => w.Value.Url.ToLowerInvariant() == currentUrl);
                currentWeb = currentWebItem?.Value;
                if (currentWeb == null || currentWeb.Url.ToLowerInvariant() == currentWebItem.Value.Key.ToLowerInvariant())
                {
                    if (currentWeb?.Url.ToLowerInvariant() == webDefinition.Url.ToLowerInvariant() &&
                        urlPartIndex == urlParts.Length - 1) return true;
                    urlPartIndex += 1;
                }
                else
                {
                    OnVerboseNotify(
                        $"Web definition Url is {currentWeb.Url} and the key is {currentWebItem.Value.Key}. This is not valid!");
                    return false;
                }
            } while (currentWeb != null && urlPartIndex < urlParts.Length);
            return false;
        }

        private void GetBaseWebDefinition(WebCreator webDefinition, bool isRootWeb)
        {
            var sourceWeb = _sourceContext.Web;
            var baseWeb = _baseContext.Web;

            //On prem SP still doesn't have the display name at this point
            try
            {
                sourceWeb.EnsureProperties(p => p.Features.Include(f => f.DefinitionId, f => f.DisplayName),
                    p => p.HasUniqueRoleAssignments, p => p.AllProperties);
                baseWeb.EnsureProperties(p => p.Features.Include(f => f.DefinitionId, f => f.DisplayName),
                    p => p.AllProperties);
            }
            catch
            {
                sourceWeb.EnsureProperties(p => p.Features, p => p.HasUniqueRoleAssignments, p => p.AllProperties);
                baseWeb.EnsureProperties(p => p.Features, p => p.AllProperties);
            }
            var canGetSourceFeatureNames = sourceWeb.Features[0].IsPropertyAvailable("DisplayName");
            var canGetBaseFeatureNames = baseWeb.Features[0].IsPropertyAvailable("DisplayName");

            webDefinition.Title = sourceWeb.Title;
            webDefinition.Description = sourceWeb.Description;
            webDefinition.Language = sourceWeb.Language;
            webDefinition.WebTemplate = $"{sourceWeb.WebTemplate}#{sourceWeb.Configuration}";

            if (isRootWeb)
            {
                GetSiteAuditSettingsAndFeatures(webDefinition, canGetSourceFeatureNames, canGetBaseFeatureNames);
                GetFeaturesToAdd(webDefinition, _sourceContext.Site.Features, _baseContext.Site.Features,
                    canGetSourceFeatureNames, FeatureDefinitionScope.Site);
                GetFeaturesToRemove(webDefinition, _baseContext.Site.Features, _sourceContext.Site.Features,
                    canGetBaseFeatureNames, FeatureDefinitionScope.Site);
                GetFieldDifferences(webDefinition);
                GetContentTypeDifferences(webDefinition);
            }
            else
            {
                OnVerboseNotify("This is a sub site.");
                AnalyzeSecurityConfiguration(webDefinition, sourceWeb);
            }

            GetFeaturesToAdd(webDefinition, sourceWeb.Features, baseWeb.Features, canGetSourceFeatureNames,
                FeatureDefinitionScope.Web);
            GetFeaturesToRemove(webDefinition, baseWeb.Features, sourceWeb.Features, canGetBaseFeatureNames,
                FeatureDefinitionScope.Web);
            GetWebProperties(webDefinition);
            GetGroupDifferences(webDefinition);
            GetRoleDefinitionDifferences(webDefinition);
            GetListDifferences(webDefinition);
        }

        private void GetListDifferences(WebCreator webDefinition)
        {
            OnVerboseNotify("Processing lists");
            _sourceContext.Web.EnsureProperties(w => w.Lists);
            _baseContext.Web.EnsureProperties(w => w.Lists);
            var listCreatorBuilder = new ListCreatorBuilder();
            listCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);

            var listsToAdd =
                _sourceContext.Web.Lists.Where(
                    l => _baseContext.Web.Lists.FirstOrDefault(bl => bl.Title == l.Title) == null);

            foreach (var list in listsToAdd)
            {
                listCreatorBuilder.GetListCreator(_sourceContext, _sourceContext.Web, list.Title,
                    webDefinition.AppManifest);
                if (_options.IncludeItemsTheseLists.Contains(list.Title))
                {
                    listCreatorBuilder.GetListCreatorListItems(_sourceContext, _sourceContext.Web, list.Title,
                        webDefinition.AppManifest);
                }
            }
        }

        private void GetRoleDefinitionDifferences(WebCreator webDefinition)
        {
            OnVerboseNotify("Processing role definitions");
            _sourceContext.Web.EnsureProperties(w => w.RoleDefinitions);
            _baseContext.Web.EnsureProperties(w => w.RoleDefinitions);
            var roleDefinitionCreatorBuilder = new RoleDefinitionCreatorBuilder();
            roleDefinitionCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);

            var roleDefinitionsToAdd =
                _sourceContext.Web.RoleDefinitions.Where(
                    rd => _baseContext.Web.RoleDefinitions.FirstOrDefault(brd => brd.Name == rd.Name) == null);

            foreach (var roleDefinition in roleDefinitionsToAdd)
            {
                roleDefinitionCreatorBuilder.GetRoleDefinitionCreator(_sourceContext, roleDefinition.Name,
                    webDefinition.AppManifest);
            }
        }

        /// <summary>
        ///     Reads the differences in groups ignoring the associated owner, etc. groups
        /// </summary>
        /// <param name="webDefinition"></param>
        private void GetGroupDifferences(WebCreator webDefinition)
        {
            OnVerboseNotify("Processing Groups");
            _sourceContext.Web.EnsureProperties(w => w.SiteGroups, w => w.AssociatedMemberGroup,
                w => w.AssociatedOwnerGroup, w => w.AssociatedVisitorGroup);
            _baseContext.Web.EnsureProperties(w => w.SiteGroups);
            var groupCreatorBuilder = new GroupCreatorBuilder();
            groupCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);

            var groupsToAdd =
                _sourceContext.Web.SiteGroups.Where(
                    g => _baseContext.Web.SiteGroups.FirstOrDefault(bg => bg.Title == g.Title) == null &&
                         g.Id != _sourceContext.Web.AssociatedMemberGroup.Id &&
                         g.Id != _sourceContext.Web.AssociatedOwnerGroup.Id &&
                         g.Id != _sourceContext.Web.AssociatedVisitorGroup.Id);

            foreach (var group in groupsToAdd)
            {
                groupCreatorBuilder.GetGroupCreator(_sourceContext, group.Title, webDefinition.AppManifest);
            }
        }

        private void GetWebProperties(WebCreator webDefinition)
        {
            var found = false;
            webDefinition.PropertyBagItems = new Dictionary<string, string>();
            foreach (var sourceProperty in _sourceContext.Web.AllProperties.FieldValues)
            {
                var exclude =
                    _options.PropertyBagExclusionPreambles.FirstOrDefault(
                        preamble => sourceProperty.Key.StartsWith(preamble, StringComparison.OrdinalIgnoreCase)) != null;

                var sourceValue = TokenizeUrls(_sourceContext.Web, sourceProperty.Value.ToString());

                if (!exclude &&
                    (!_baseContext.Web.AllProperties.FieldValues.ContainsKey(sourceProperty.Key) ||
                     TokenizeUrls(_baseContext.Web,
                         _baseContext.Web.AllProperties.FieldValues[sourceProperty.Key].ToString()) !=
                     sourceValue))
                {
                    Guid outGuid;
                    //If the value is a guid, ignore it
                    if (!Guid.TryParse(sourceValue, out outGuid))
                    {
                        webDefinition.PropertyBagItems[sourceProperty.Key] = sourceValue;
                        OnVerboseNotify($"Found property bag difference. Setting {sourceProperty.Key} to {sourceValue}");
                        found = true;
                    }
                }
            }
            if (!found)
            {
                OnVerboseNotify(
                    "No property bag differences found. If you know there are differences, make sure the you set the exclusion options as needed.");
            }
        }

        private void GetContentTypeDifferences(WebCreator webDefinition)
        {
            OnVerboseNotify("Processing Content Types");
            webDefinition.AppManifest.ContentTypeCreators = new Dictionary<string, ContentTypeCreator>();
            _sourceContext.Web.EnsureProperty(w => w.ContentTypes.Include(ct => ct.Name));
            _baseContext.Web.EnsureProperty(w => w.ContentTypes.Include(ct => ct.Name));
            var contentTypeCreatorBuilder = new ContentTypeCreatorBuilder();
            contentTypeCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);

            var contentTypesToAdd =
                _sourceContext.Web.ContentTypes.Where(
                    ct => _baseContext.Web.ContentTypes.FirstOrDefault(bct => bct.Name == ct.Name) == null);
            foreach (var contentType in contentTypesToAdd)
            {
                contentTypeCreatorBuilder.GetContentTypeCreator(_sourceContext, contentType.Name,
                    webDefinition.AppManifest);
            }
        }

        private void GetFieldDifferences(WebCreator webDefinition)
        {
            OnVerboseNotify("Processing fields");
            webDefinition.AppManifest.Fields = new Dictionary<string, string>();
            _sourceContext.Web.EnsureProperty(w => w.Fields.Include(f => f.InternalName, f => f.Sealed, f => f.FieldTypeKind, f => f.Hidden, f => f.Title));
            _baseContext.Web.EnsureProperty(w => w.Fields.Include(f => f.InternalName));
            var fieldCreatorBuilder = new FieldCreatorBuilder();
            fieldCreatorBuilder.VerboseNotify += (sender, args) => OnVerboseNotify(args.Message);

            var fieldsToAdd =
                _sourceContext.Web.Fields.Where(
                    f => _baseContext.Web.Fields.FirstOrDefault(bf => bf.InternalName == f.InternalName) == null);
            foreach (var field in fieldsToAdd)
            {
                if (field.Sealed)
                {
                    OnVerboseNotify(
                        $"Sealed field {field.InternalName} found. Skipping. If this field is needed, add it explicitly. Note that if you install a sealed field, there is no way to uninstall it.");
                }
                else
                {
                    Guid possibleTaxonomyField;
                    //Somebody thought it would be a good idea to replace the first digit of the guid with a random character under certain circumstances
                    //So replacing the first characte with a 0 will turn it back into a guid, if it is a guid
                    var possibleTaxonomyFieldInternalName = $"0{field.InternalName.Substring(1)}";

                    if (field.FieldTypeKind == FieldType.Note && field.Hidden &&
                        Guid.TryParseExact(possibleTaxonomyFieldInternalName, "N", out possibleTaxonomyField) &&
                        field.Title.Contains("_"))
                    {
                        OnVerboseNotify(
                            $"Skipping taxonomy note field {field.InternalName} found. Skipping. If this field is needed, add it explicitly.");
                    }
                    else
                    {
                        fieldCreatorBuilder.GetFieldCreator(_sourceContext, field.InternalName,
                            webDefinition.AppManifest);
                    }
                }
            }
        }

        private void GetSiteAuditSettingsAndFeatures(WebCreator webDefinition, bool canGetSourceFeatureNames,
            bool canGetBaseFeatureNames)
        {
            OnVerboseNotify("This is a root web.");

            //On prem SP still doesn't have the display name at this point
            if (canGetSourceFeatureNames)
            {
                _sourceContext.Site.EnsureProperties(p => p.Audit, p => p.AuditLogTrimmingRetention,
                    p => p.TrimAuditLog, p => p.Features.Include(f => f.DefinitionId, f => f.DisplayName));
            }
            else
            {
                _sourceContext.Site.EnsureProperties(p => p.Audit, p => p.AuditLogTrimmingRetention,
                    p => p.TrimAuditLog, p => p.Features);
            }
            if (canGetBaseFeatureNames)
            {
                _baseContext.Site.EnsureProperties(p => p.Audit, p => p.AuditLogTrimmingRetention,
                    p => p.TrimAuditLog, p => p.Features.Include(f => f.DefinitionId, f => f.DisplayName));
            }
            else
            {
                _baseContext.Site.EnsureProperties(p => p.Audit, p => p.AuditLogTrimmingRetention,
                    p => p.TrimAuditLog, p => p.Features);
            }

            if (_sourceContext.Site.Audit.AuditFlags != _baseContext.Site.Audit.AuditFlags ||
                _sourceContext.Site.AuditLogTrimmingRetention != _baseContext.Site.AuditLogTrimmingRetention ||
                _sourceContext.Site.TrimAuditLog != _baseContext.Site.TrimAuditLog)
            {
                OnVerboseNotify("Found differences in audit settings.");
                webDefinition.SiteAuditSettings = new SiteAuditSettings
                {
                    AuditMaskType = _sourceContext.Site.Audit.AuditFlags,
                    AuditLogTrimmingRetention = _sourceContext.Site.AuditLogTrimmingRetention,
                    TrimAuditLog = _sourceContext.Site.TrimAuditLog
                };
            }
            else
            {
                OnVerboseNotify("Did not find differences in audit settings.");
            }

            if (!canGetSourceFeatureNames)
            {
                OnVerboseNotify(
                    "Unable to get feature display names from the source context's CSOM implementation. Using the id instead, Sorry...");
            }
            if (!canGetBaseFeatureNames)
            {
                OnVerboseNotify(
                    "Unable to get feature display names from the base context's CSOM implementation. Using the id instead, Sorry...");
            }
        }

        private void GetFeaturesToRemove(WebCreator webDefinition, FeatureCollection baseFeatures,
            FeatureCollection sourceFeatures, bool canGetBaseFeatureNames, FeatureDefinitionScope featureScope)
        {
            webDefinition.AppManifest.RemoveFeatures = new Dictionary<string, FeatureRemoverCreator>();
            var foundRemoveFeatures = false;
            foreach (var baseFeature in baseFeatures)
            {
                var sourceFeature =
                    sourceFeatures.FirstOrDefault(f => f.DefinitionId == baseFeature.DefinitionId);

                if (sourceFeature == null)
                {
                    foundRemoveFeatures = true;
                    var name = canGetBaseFeatureNames ? baseFeature.DisplayName : baseFeature.DefinitionId.ToString();
                    OnVerboseNotify($"Found {featureScope} scoped feature to remove {name}.");
                    webDefinition.AppManifest.RemoveFeatures[name] = new FeatureRemoverCreator
                    {
                        DisplayName = name,
                        FeatureDefinitionScope = featureScope,
                        FeatureId = baseFeature.DefinitionId,
                        Force = true
                    };
                }
            }
            if (foundRemoveFeatures)
            {
                OnVerboseNotify(
                    "Found features to remove. Note that the support for features using CSOM is pretty limited and there is no guarantee they are in the proper order in the manifest. You should test and adjust as needed!");
            }
        }

        private void GetFeaturesToAdd(WebCreator webDefinition, FeatureCollection sourceFeatures,
            FeatureCollection baseFeatures,
            bool canGetSourceFeatureNames, FeatureDefinitionScope featureScope)
        {
            webDefinition.AppManifest.AddFeatures = new Dictionary<string, FeatureAdderCreator>();
            var foundAddFeatures = false;
            foreach (var sourceFeature in sourceFeatures)
            {
                var baseFeature =
                    baseFeatures.FirstOrDefault(f => f.DefinitionId == sourceFeature.DefinitionId);

                if (baseFeature == null)
                {
                    foundAddFeatures = true;
                    var name = canGetSourceFeatureNames
                        ? sourceFeature.DisplayName
                        : sourceFeature.DefinitionId.ToString();
                    OnVerboseNotify($"Found {featureScope} scoped feature to add {name}.");
                    webDefinition.AppManifest.AddFeatures[name] = new FeatureAdderCreator
                    {
                        DisplayName = name,
                        FeatureDefinitionScope = featureScope,
                        FeatureId = sourceFeature.DefinitionId,
                        Force = true
                    };
                }
            }
            if (foundAddFeatures)
            {
                OnVerboseNotify(
                    "Found features to add. Note that the support for features using CSOM is pretty limited and there is no guarantee they are in the proper order in the manifest. You should test and adjust as needed!");
            }
        }

        private void AnalyzeSecurityConfiguration(WebCreator webDefinition, Web web)
        {
            if (web.HasUniqueRoleAssignments)
            {
                OnVerboseNotify("Web has unique role assignments. Analyzing security configuration.");
                web.EnsureProperties(w => w.ParentWeb,
                    w => w.RoleAssignments.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings));
                var parentWeb = _sourceContext.Site.OpenWebById(web.ParentWeb.Id);
                parentWeb.EnsureProperties(
                    w => w.RoleAssignments.Include(ra => ra.Member, ra => ra.RoleDefinitionBindings));

                if (webDefinition.SecurityConfiguration == null)
                {
                    webDefinition.SecurityConfiguration = new SecureObjectCreator
                    {
                        SecureObjectType = SecureObjectType.Web,
                        Title = webDefinition.Title,
                        Url = "/",
                        GroupRoleDefinitions = new Dictionary<string, string>()
                    };
                }

                webDefinition.SecurityConfiguration.Title = webDefinition.Title;
                webDefinition.SecurityConfiguration.Url = webDefinition.Url;
                webDefinition.SecurityConfiguration.BreakInheritance = true;

                //First loop thorugh and see if the parent has principals with assignments not found in the web
                CheckShouldCopyExistingPermissions(webDefinition, web, parentWeb);

                //Next loop through the assignments on the web and build the output
                FillWebGroupRoleDefinitions(webDefinition, web, parentWeb);
            }
        }

        private void CheckShouldCopyExistingPermissions(WebCreator webDefinition, Web web, Web parentWeb)
        {
            webDefinition.SecurityConfiguration.CopyExisting = true;
            foreach (var roleAssignment in parentWeb.RoleAssignments)
            {
                var principal = roleAssignment.Member;
                if (principal.PrincipalType == PrincipalType.SharePointGroup)
                {
                    var matchingMember = web.RoleAssignments.FirstOrDefault(w => w.Member.Id == roleAssignment.Member.Id);
                    //The first unique ancestor has an assignment that isn't in the web, assume break inheritance
                    if (matchingMember == null)
                    {
                        webDefinition.SecurityConfiguration.CopyExisting = false;
                        OnVerboseNotify("Determined should not copy existing permissions from parent.");
                        break;
                    }
                }
            }
        }

        private void FillWebGroupRoleDefinitions(WebCreator creator, Web web, Web parentWeb)
        {
            _sourceContext.Load(web.AssociatedMemberGroup, g => g.Id);
            _sourceContext.Load(web.AssociatedOwnerGroup, g => g.Id);
            _sourceContext.Load(web.AssociatedVisitorGroup, g => g.Id);
            _sourceContext.ExecuteQueryRetry();

            foreach (var roleAssignment in web.RoleAssignments)
            {
                var principal = roleAssignment.Member;
                var principalName = principal.LoginName;
                var principalId = principal.Id;

                if (principalId == web.AssociatedMemberGroup.Id)
                {
                    principalName = "AssociatedMemberGroup";
                }
                if (principalId == web.AssociatedOwnerGroup.Id)
                {
                    principalName = "AssociatedOwnerGroup";
                }
                if (principalId == web.AssociatedVisitorGroup.Id)
                {
                    principalName = "AssociatedVisitorGroup";
                }

                if (principal.PrincipalType == PrincipalType.SharePointGroup ||
                    principal.PrincipalType == PrincipalType.SecurityGroup)
                {
                    foreach (var roleDefinition in roleAssignment.RoleDefinitionBindings)
                    {
                        //This part of the object model is quirky
                        //There should be at most two for a given principal
                        //but if there are more the first one that isn't Limited Access wins
                        if (roleDefinition.Name != "Limited Access")
                        {
                            if (!creator.SecurityConfiguration.CopyExisting)
                            {
                                if (!creator.SecurityConfiguration.GroupRoleDefinitions.ContainsKey(principalName))
                                {
                                    OnVerboseNotify(
                                        $"Adding princiapl {principalName} to role definition {roleDefinition.Name}");
                                    creator.SecurityConfiguration.GroupRoleDefinitions.Add(principalName,
                                        roleDefinition.Name);
                                }
                            }
                            else
                            {
                                var inFirstAncestor = false;
                                foreach (var parentWebRoleAssignment in parentWeb.RoleAssignments)
                                {
                                    if (parentWebRoleAssignment.Member.LoginName == principalName)
                                    {
                                        foreach (
                                            var parentWebRoleDefinition in
                                                parentWebRoleAssignment.RoleDefinitionBindings)
                                        {
                                            if (roleDefinition.Name == parentWebRoleDefinition.Name)
                                            {
                                                inFirstAncestor = true;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                if (!inFirstAncestor)
                                {
                                    if (!creator.SecurityConfiguration.GroupRoleDefinitions.ContainsKey(principalName))
                                    {
                                        OnVerboseNotify(
                                            $"Adding princiapl {principalName} to role definition {roleDefinition.Name}");
                                        creator.SecurityConfiguration.GroupRoleDefinitions.Add(principalName,
                                            roleDefinition.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}