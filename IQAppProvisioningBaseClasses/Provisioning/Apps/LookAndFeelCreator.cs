namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class LookAndFeelCreator
    {
        public virtual string SiteTitle { get; set; }
        public virtual string SiteLogoUrl { get; set; }
        public virtual string DefaultMasterPageUrl { get; set; }
        public virtual string CustomMasterPageUrl { get; set; }
        public virtual string AlternateCssUrl { get; set; }
        public virtual ListItemCreator CurrentComposedLook { get; set; }
    }
}