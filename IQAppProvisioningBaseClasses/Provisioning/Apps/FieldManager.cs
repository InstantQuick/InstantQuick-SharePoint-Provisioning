using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class FieldManager : ProvisioningManagerBase
    {
        private ClientContext _ctx;

        private List<string> _existingFields;
        private Web _targetWeb;
        public virtual Dictionary<string, string> FieldDefinitions { get; set; } = null;

        public void CreateAll(ClientContext ctx)
        {
            _ctx = ctx;
            //Install to rootweb if not a hostweb
            _targetWeb = _ctx.Web.AppInstanceId == default(Guid) ? _ctx.Site.RootWeb : _ctx.Web;

            if (FieldDefinitions != null && FieldDefinitions.Count > 0)
            {
                try
                {
                    GetExistingFields();

                    foreach (var key in FieldDefinitions.Keys)
                    {
                        if (!_existingFields.Contains(key))
                        {
                            //The Version attribute is BAD. Let SharePoint manage it. 
                            var schemaXml = FieldDefinitions[key].RemoveXmlAttribute("Version");
                            schemaXml = FieldTokenizer.DoTokenReplacement(_ctx, schemaXml);
                            CleanupTaxonomyHiddenField(schemaXml);
                            _targetWeb.Fields.AddFieldAsXml(schemaXml, true, AddFieldOptions.AddToNoContentType);
                            OnNotify(ProvisioningNotificationLevels.Verbose, "Adding field " + key);
                        }
                        else
                        {
                            OnNotify(ProvisioningNotificationLevels.Verbose, "Skipped existing field " + key);
                        }
                    }
                    _ctx.ExecuteQueryRetry();
                }
                catch (Exception ex)
                {
                    OnNotify(ProvisioningNotificationLevels.Normal,
                        "Error creating fields at " + ctx.Site.RootWeb.Url + " | " + ex);
                    Trace.TraceError("Error creating fields at " + ctx.Site.RootWeb.Url + " | " + ex);
                    throw;
                }
            }
        }

        private void CleanupTaxonomyHiddenField(string schemaXml)
        {
            try
            {
                var fieldType = schemaXml.GetXmlAttribute("Type");
                var noteDisplayName = $"{schemaXml.GetXmlAttribute("Name")}_0";
                if (fieldType.StartsWith("TaxonomyField"))
                {
                    var fieldId = Guid.Parse(schemaXml.GetXmlAttribute("ID")).ToString("N");
                    Field deleteNoteField = null;
                    if (_existingFields.Contains(fieldId))
                    {
                        deleteNoteField = _targetWeb.Fields.GetByInternalNameOrTitle(fieldId);
                    }
                    else
                    {
                        //Might not exist and can cause an error that should be ignored
                        deleteNoteField = _targetWeb.Fields.GetByTitle(noteDisplayName);
                    }
                    deleteNoteField.DeleteObject();
                    _ctx.ExecuteQuery();
                }
            }
            catch
            {
                //ignore
            }
        }

        public void DeleteAll(ClientContext ctx)
        {
            _ctx = ctx;
            //Install to rootweb if not a hostweb
            _targetWeb = _ctx.Web.AppInstanceId == default(Guid) ? _ctx.Site.RootWeb : _ctx.Web;
            if (FieldDefinitions != null && FieldDefinitions.Count > 0)
            {
                GetExistingFields();

                foreach (var key in FieldDefinitions.Keys)
                {
                    if (_existingFields.Contains(key))
                    {
                        OnNotify(ProvisioningNotificationLevels.Verbose, "Removing field " + key);
                        var f = _targetWeb.Fields.GetByInternalNameOrTitle(key);
                        f.DeleteObject();
                    }
                }
                _ctx.ExecuteQueryRetry();
            }
        }

        public void GetExistingFields()
        {
            _existingFields = new List<string>();

            var fields = _ctx.Web.AvailableFields;

            _ctx.Load(fields,
                f => f.Include
                    (field => field.InternalName, field => field.SchemaXml));

            _ctx.ExecuteQueryRetry();

            _existingFields = FillExistingFieldsList(fields);
        }

        public void GetExistingFields(string group)
        {
            var fields = _ctx.Web.AvailableFields;

            _ctx.Load(fields,
                f => f.Include
                    (field => field.InternalName, field => field.SchemaXml).Where
                    (field => field.Group == group || field.Group == "_Hidden"));

            _ctx.ExecuteQueryRetry();

            _existingFields = FillExistingFieldsList(fields);
        }

        private List<string> FillExistingFieldsList(FieldCollection fields)
        {
            var retList = new List<string>();
            foreach (var field in fields)
            {
                retList.Add(field.InternalName);
            }
            return retList;
        }
    }
}