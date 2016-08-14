using IQAppManifestBuilders;
using IQAppProvisioningBaseClasses.Events;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    public enum CreatorTypes
    {
        Field,
        ContentType,
        List,
        File,
        RoleDefinition,
        Group,
        CustomAction,
        Navigation,
        RemoteEvents,
        LookAndFeel
    }

    public class CreatorBuilder : CreatorBuilderBase
    {
        public string GetCreator(ClientContext ctx, Web web, string title, AppManifestBase manifest,
            CreatorTypes creatorType)
        {
            OnVerboseNotify("START Getting " + title);
            switch (creatorType)
            {
                case CreatorTypes.Field:
                    return GetFieldCreator(ctx, title, manifest);
                case CreatorTypes.ContentType:
                    return GetContentTypeCreator(ctx, title, manifest);
                case CreatorTypes.List:
                    return GetListCreator(ctx, web, title, manifest);
                case CreatorTypes.RoleDefinition:
                    return GetRoleDefinitionCreator(ctx, title, manifest);
                case CreatorTypes.Group:
                    return GetGroupCreator(ctx, title, manifest);
                case CreatorTypes.Navigation:
                    return GetNavigationCreator(ctx, web, title, manifest);
                case CreatorTypes.RemoteEvents:
                    return GetRemoteEventRegistrationsCreator(ctx, web, manifest);
                case CreatorTypes.LookAndFeel:
                    return GetLookAndFeelCreator(ctx, web, manifest);
            }
            OnVerboseNotify("END Getting " + title);
            return string.Empty;
        }

        private string GetLookAndFeelCreator(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting look and feel");

            var builder = new LookAndFeelCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetLookAndFeelCreator(ctx);
            }
            builder.GetLookAndFeelCreator(ctx, web, manifest);
            return string.Empty;
        }

        private string GetRemoteEventRegistrationsCreator(ClientContext ctx, Web web, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting remote events for web");

            var builder = new RemoteEventRegistrationCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetRemoteEventRegistrationCreators(ctx, web);
            }
            builder.GetRemoteEventRegistrationCreators(ctx, web, manifest);
            return string.Empty;
        }

        private string GetNavigationCreator(ClientContext ctx, Web web, string navigationCollection,
            AppManifestBase manifest)
        {
            OnVerboseNotify("Getting navigation for " + navigationCollection);

            var builder = new NavigationCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetNavigationCreator(ctx, web, navigationCollection);
            }
            builder.GetNavigationCreator(ctx, web, navigationCollection, manifest);
            return string.Empty;
        }

        private string GetGroupCreator(ClientContext ctx, string title, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting group definition for " + title);

            var builder = new GroupCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetGroupCreator(ctx, title);
            }
            builder.GetGroupCreator(ctx, title, manifest);
            return string.Empty;
        }

        private string GetRoleDefinitionCreator(ClientContext ctx, string title, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting role definition for " + title);

            var builder = new RoleDefinitionCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetRoleDefinitionCreator(ctx, title);
            }
            builder.GetRoleDefinitionCreator(ctx, title, manifest);
            return string.Empty;
        }

        private string GetListCreator(ClientContext ctx, Web web, string title, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting content type creator for " + title);

            var builder = new ListCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetListCreator(ctx, web, title);
            }
            builder.GetListCreator(ctx, web, title, manifest);
            return string.Empty;
        }

        private string GetContentTypeCreator(ClientContext ctx, string title, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting content type creator for " + title);

            var builder = new ContentTypeCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetContentTypeCreator(ctx, title);
            }
            builder.GetContentTypeCreator(ctx, title, manifest);
            return string.Empty;
        }

        private void builder_Notify(object sender, CreatorBuilderProgressNotificationEvent eventArgs)
        {
            OnVerboseNotify(eventArgs.Message);
        }

        private string GetFieldCreator(ClientContext ctx, string title, AppManifestBase manifest)
        {
            OnVerboseNotify("Getting field creator for " + title);

            var builder = new FieldCreatorBuilder();
            builder.VerboseNotify += builder_Notify;
            if (manifest == null)
            {
                return builder.GetFieldCreator(ctx, title);
            }
            builder.GetFieldCreator(ctx, title, manifest);
            return string.Empty;
        }
    }
}