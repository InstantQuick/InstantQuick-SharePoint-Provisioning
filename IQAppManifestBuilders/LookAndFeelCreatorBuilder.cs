using System;
using System.Linq;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;
using static IQAppProvisioningBaseClasses.Utility.Tokenizer;

namespace IQAppManifestBuilders
{
    public class LookAndFeelCreatorBuilder : CreatorBuilderBase
    {
        public string GetLookAndFeelCreator(ClientContext ctx)
        {
            return GetLookAndFeelCreator(ctx, ctx.Web);
        }

        public string GetLookAndFeelCreator(ClientContext ctx, Web web)
        {
            var js = new JavaScriptSerializer();
            var lookAndFeel = GetLookAndFeel(ctx, web);

            return js.Serialize(lookAndFeel);
        }

        public void GetLookAndFeelCreator(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            manifest.LookAndFeel = GetLookAndFeel(ctx, web);
        }

        private LookAndFeelCreator GetLookAndFeel(ClientContext ctx)
        {
            return GetLookAndFeel(ctx, ctx.Web);
        }

        private LookAndFeelCreator GetLookAndFeel(ClientContext ctx, Web web)
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

            ctx.Load(web);
            ctx.ExecuteQueryRetry();
            var lookAndFeel = new LookAndFeelCreator
            {
                //Can't decide if it should ever set the site title, but leaving the option
                //You just have to specify it in the manifest manually
                //SiteTitle = web.Title,
                SiteLogoUrl = web.SiteLogoUrl,
                AlternateCssUrl = web.AlternateCssUrl,
                DefaultMasterPageUrl = web.MasterUrl,
                CustomMasterPageUrl = web.CustomMasterUrl
            };

            lookAndFeel.SiteLogoUrl = TokenizeUrls(web, lookAndFeel.SiteLogoUrl ?? "");
            lookAndFeel.AlternateCssUrl = TokenizeUrls(web, lookAndFeel.AlternateCssUrl ?? "");
            lookAndFeel.DefaultMasterPageUrl = TokenizeUrls(web, lookAndFeel.DefaultMasterPageUrl ?? "");
            lookAndFeel.CustomMasterPageUrl = TokenizeUrls(web, lookAndFeel.CustomMasterPageUrl ?? "");

            var composedLooks = web.GetCatalog((int) ListTemplateType.DesignCatalog);
            var query = new CamlQuery {ViewXml = composedLookQuery};
            var current = composedLooks.GetItems(query);
            ctx.Load(current, items => items.Include(i => i.FieldValuesForEdit, i => i.FieldValuesAsText, i => i.FieldValuesAsHtml, i => i.ContentType));

            //{67df98f4-9dec-48ff-a553-29bece9c5bf4} is Attachments
            var attachmentsFieldId = Guid.Parse("{67df98f4-9dec-48ff-a553-29bece9c5bf4}");
            ctx.Load(composedLooks.Fields, fields => fields.Where(f => (!f.Hidden && !f.ReadOnlyField && f.Id != attachmentsFieldId) || f.InternalName == "ContentTypeId"));

            ctx.ExecuteQueryRetry();
            var item = current.FirstOrDefault();

            if (item != null)
            {
                lookAndFeel.CurrentComposedLook =
                    new ListItemCreatorBuilder(ctx, web, composedLooks).GetListItemCreator(item);
            }
            return lookAndFeel;
        }
    }
}