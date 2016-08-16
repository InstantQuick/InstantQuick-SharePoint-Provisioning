using System.Collections.Generic;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class WebCreator
    {
        public static readonly string DefaultTitle = "Default Title";
        public static readonly string DefaultDescription = string.Empty;
        public static readonly uint DefaultLanguage = 1033;

        /// <summary>
        /// Contains detailed provisioning instructions for the web
        /// </summary>
        public virtual AppManifestBase AppManifest { get; set; } = new AppManifestBase();
        
        public virtual string Description { get; set; } = DefaultDescription;
        public virtual uint Language { get; set; } = DefaultLanguage;
        public virtual string Title { get; set; } = DefaultTitle;

        /// <summary>
        ///     Site audit settings, applied when provisioning to root web only
        /// </summary>
        public virtual SiteAuditSettings SiteAuditSettings { get; set; }

        /// <summary>
        ///     Url relative to the root of site definition
        ///     The root of the site definition should be "/"
        /// </summary>
        /// <remarks>
        ///     If the containing site definition uses file storage
        ///     the Url provides the base file path for the web's app manifest 
        /// </remarks>
        public virtual string Url { get; set; }

        /// <summary>
        /// Security configuration is applied after all app manifests have been applied as it may be necessary for
        /// role definitions and groups to exist prior to applying the final configuartion
        /// </summary>
        public virtual SecureObjectCreator SecurityConfiguration { get; set; }

        /// <summary>
        /// The key is the site definition relative url
        /// The value is the web definition
        /// </summary>
        public virtual Dictionary<string, WebCreator> Webs { get; set; } = new Dictionary<string, WebCreator>();

        public virtual string WebTemplate { get; set; }

        /// <summary>
        ///     Property bag entries for the web
        /// </summary>
        public virtual Dictionary<string, string> PropertyBagItems { get; set; } = new Dictionary<string, string>();
    }
}