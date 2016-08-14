using System.Collections.Generic;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ContentTypeCreator
    {
        /// <summary>
        ///     A list of fields to add to the parent type.
        ///     It excludes inherited fields
        /// </summary>
        public virtual List<string> Fields { get; set; }

        /// <summary>
        ///     A list of ordered fields including inherited fields
        /// </summary>
        public virtual List<string> OrderedFields { get; set; }

        /// <summary>
        ///     A list of inherited fields to remove
        /// </summary>
        public virtual List<string> RemoveFields { get; set; }
        public virtual string Id { get; set; }
        public virtual string ParentContentTypeName { get; set; }
        public virtual string Description { get; set; }
        public virtual string Group { get; set; }
        public virtual string BaseViewUrl { get; set; }
        public virtual string NewFormUrl { get; set; }
        public virtual bool NewFormIsDialog { get; set; }
        public virtual string NewFormDialogTitle { get; set; }
        public virtual int? NewFormDialogHeight { get; set; }
        public virtual int? NewFormDialogWidth { get; set; }
        public virtual string EditFormUrl { get; set; }
        public virtual bool EditFormIsDialog { get; set; }
        public virtual string EditFormDialogTitle { get; set; }
        public virtual int? EditFormDialogHeight { get; set; }
        public virtual int? EditFormDialogWidth { get; set; }
        public virtual string DisplayFormUrl { get; set; }
        public virtual bool DisplayFormIsDialog { get; set; }
        public virtual string DisplayFormDialogTitle { get; set; }
        public virtual int? DisplayFormDialogHeight { get; set; }
        public virtual int? DisplayFormDialogWidth { get; set; }
        public ContentType ContentType { get; set; }
    }
}