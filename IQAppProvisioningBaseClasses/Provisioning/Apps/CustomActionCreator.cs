using Microsoft.SharePoint.Client;

// ReSharper disable InconsistentNaming

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class CustomActionCreator
    {
        public virtual bool SiteScope { get; set; }
        public virtual string CommandUIExtension { get; set; }
        public virtual string Description { get; set; }
        public virtual string Group { get; set; }
        public virtual string ImageUrl { get; set; }
        public virtual string Location { get; set; }
        public virtual string RegistrationId { get; set; }
        public virtual UserCustomActionRegistrationType RegistrationType { get; set; }
        //public virtual BasePermissions Rights { get { return new BasePermissions(); } }
        public virtual string ClientId { get; set; }
        public virtual string Version { get; set; }
        public virtual string ScriptBlock { get; set; }
        public virtual string ScriptSrc { get; set; }
        public virtual int Sequence { get; set; }
        public virtual string Title { get; set; }
        public virtual string Url { get; set; }
    }
}