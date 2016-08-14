using System;
using System.Linq;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;
using static IQAppProvisioningBaseClasses.Utility.Tokenizer;

namespace IQAppProvisioningBaseClasses.Provisioning
{

    public class LookAndFeelManager : ProvisioningManagerBase
    {
        private class ValuesToApply
        {
            public string SiteTitle { get; set; }
            public string SiteLogoUrl { get; set; }
            public string DefaultMasterPageUrl { get; set; }
            public string CustomMasterPageUrl { get; set; }
            public string AlternateCssUrl { get; set; }
            public string ThemeUrl { get; set; }
            public string ImageUrl { get; set; }
            public string FontSchemeUrl { get; set; }
        }

        public void ProvisionLookAndFeel(AppManifestBase manifest, ClientContext ctx, Web web)
        {
            if (manifest.LookAndFeel == null) return;

            ValuesToApply valuesToApply = GetValuesToApply(manifest.LookAndFeel, web);

            //Update the current composed look if there is one
            UpdateCurrentComposedLook(valuesToApply, ctx, web);

            //Then override the current look based on what the branding properties are actually set to, if needed
            ApplyBranding(valuesToApply, ctx, web);
        }

        private ValuesToApply GetValuesToApply(LookAndFeelCreator lookAndFeel, Web web)
        {
            var valuesToApply = new ValuesToApply();

            valuesToApply.SiteTitle = lookAndFeel.SiteTitle;
            valuesToApply.AlternateCssUrl = ReplaceUrlTokens(web, lookAndFeel.AlternateCssUrl);
            valuesToApply.CustomMasterPageUrl = ReplaceUrlTokens(web, lookAndFeel.CustomMasterPageUrl);
            valuesToApply.DefaultMasterPageUrl = ReplaceUrlTokens(web, lookAndFeel.DefaultMasterPageUrl);
            valuesToApply.SiteLogoUrl = ReplaceUrlTokens(web, lookAndFeel.SiteLogoUrl);
            if (lookAndFeel.CurrentComposedLook != null)
            {
                valuesToApply.ThemeUrl =
                    GetThemeUrlPart(lookAndFeel.CurrentComposedLook.FieldValues.FirstOrDefault(fv => fv.FieldName == "ThemeUrl")?.Value, web);
                valuesToApply.ImageUrl =
                    GetThemeUrlPart(lookAndFeel.CurrentComposedLook.FieldValues.FirstOrDefault(fv => fv.FieldName == "ImageUrl")?.Value, web);
                valuesToApply.FontSchemeUrl =
                    GetThemeUrlPart(lookAndFeel.CurrentComposedLook.FieldValues.FirstOrDefault(fv => fv.FieldName == "FontSchemeUrl")?.Value, web);
            }

            return valuesToApply;
        }

        private string GetThemeUrlPart(string value, Web web)
        {
            //Very important this returns null, empty string will throw
            if (string.IsNullOrEmpty(value)) return null;

            //Because URLs in the SP object model are crazy inconsistent
            return value.Split(',')[0].Trim().Replace("{@WebUrl}", web.ServerRelativeUrl);
        }

        private void ApplyBranding(ValuesToApply valuesToApply, ClientContext ctx, Web web)
        {
            var shouldExecute = false;

            if (!string.IsNullOrEmpty(valuesToApply.SiteTitle))
            {
                shouldExecute = true;
                web.Title = valuesToApply.SiteTitle;
                Console.WriteLine("Set web title to " + valuesToApply.SiteTitle);
            }
            if (!string.IsNullOrEmpty(valuesToApply.SiteLogoUrl))
            {
                shouldExecute = true;
                web.SiteLogoUrl = valuesToApply.SiteLogoUrl;
                OnNotify(ProvisioningNotificationLevels.Verbose, $"Set web logo to {web.SiteLogoUrl}");
            }
            if (!string.IsNullOrEmpty(valuesToApply.DefaultMasterPageUrl))
            {
                shouldExecute = true;
                web.MasterUrl = valuesToApply.DefaultMasterPageUrl;
                OnNotify(ProvisioningNotificationLevels.Verbose, $"Set web default master page url to {web.MasterUrl}");
            }
            if (!string.IsNullOrEmpty(valuesToApply.CustomMasterPageUrl))
            {
                shouldExecute = true;
                web.CustomMasterUrl = valuesToApply.CustomMasterPageUrl;
                OnNotify(ProvisioningNotificationLevels.Verbose, $"Set web default master page url to {web.CustomMasterUrl}");
            }
            if (!string.IsNullOrEmpty(valuesToApply.AlternateCssUrl))
            {
                shouldExecute = true;
                web.AlternateCssUrl = valuesToApply.AlternateCssUrl;
                OnNotify(ProvisioningNotificationLevels.Verbose, $"Set web alternate css url to {web.AlternateCssUrl}");
            }
            if (shouldExecute)
            {
                web.Update();
                try
                {
                    ctx.ExecuteQueryRetry();
                    shouldExecute = false;
                }
                catch
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Failed to apply branding. Make sure all the files you need are either in the site or the package. Note that images for composed looks are often the cause of this problem.");
                }
            }
            if (!string.IsNullOrEmpty(valuesToApply.ThemeUrl))
            {
                shouldExecute = true;
                web.ApplyTheme(valuesToApply.ThemeUrl, valuesToApply.FontSchemeUrl, valuesToApply.ImageUrl, false);
                OnNotify(ProvisioningNotificationLevels.Verbose, $"Set theme to {valuesToApply.ThemeUrl}");
            }
            if (shouldExecute)
            {
                web.Update();
                try
                {
                    ctx.ExecuteQueryRetry();
                }
                catch
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Failed to apply branding. Make sure all the files you need are either in the site or the package. Note that images for composed looks are often the cause of this problem.");
                }
            }
        }

        private void UpdateCurrentComposedLook(ValuesToApply valuesToApply, ClientContext ctx, Web web)
        {
            var composedLookQuery = @"<View>
                    <Query>
                    <Where>
                        <Eq>
                        <FieldRef Name='Name' />
                        <Value Type='Text'>Current</Value>
                        </Eq>
                    </Where>
                    </Query>
                    <ViewFields>
                    <FieldRef Name='Name' />
                    <FieldRef Name='MasterPageUrl' />
                    <FieldRef Name='ThemeUrl' />
                    <FieldRef Name='ImageUrl' />
                    <FieldRef Name='FontSchemeUrl' />
                    <FieldRef Name='DisplayOrder' />
                    </ViewFields>
                </View>";

            var composedLooks = web.GetCatalog((int) ListTemplateType.DesignCatalog);
            var query = new CamlQuery {ViewXml = composedLookQuery};
            var current = composedLooks.GetItems(query);
            ctx.Load(current);
            ctx.ExecuteQueryRetry();

            var item = current.FirstOrDefault();

            if (item != null)
            {
                item["MasterPageUrl"] = valuesToApply.DefaultMasterPageUrl != null ? $"{valuesToApply.DefaultMasterPageUrl}, {valuesToApply.DefaultMasterPageUrl.Replace(web.Url, web.ServerRelativeUrl)}" : null;
                item["ThemeUrl"] = valuesToApply.ThemeUrl != null ? $"{valuesToApply.ThemeUrl}, {valuesToApply.ThemeUrl.Replace(web.Url, web.ServerRelativeUrl)}" : null;
                item["ImageUrl"] = valuesToApply.ImageUrl != null ? $"{valuesToApply.ImageUrl}, {valuesToApply.ImageUrl.Replace(web.Url, web.ServerRelativeUrl)}" : null;
                item["FontSchemeUrl"] = valuesToApply.FontSchemeUrl != null ? $"{valuesToApply.FontSchemeUrl}, {valuesToApply.FontSchemeUrl.Replace(web.Url, web.ServerRelativeUrl)}" : null;
                item.Update();
                ctx.ExecuteQueryRetry();
            }
        }
    }
}