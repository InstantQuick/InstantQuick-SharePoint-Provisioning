using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    public class FieldCreatorBuilder : CreatorBuilderBase
    {
        public string GetFieldCreator(ClientContext ctx, string fieldName)
        {
            try
            {
                var retVal = new Dictionary<string, string>();
                var js = new JavaScriptSerializer();
                var field = GetField(ctx, fieldName);
                if (field == null)
                {
                    OnVerboseNotify($"No information found for {fieldName}");
                    return string.Empty;
                }
                var schemaXml = FieldTokenizer.DoTokenSubstitutions(ctx, field);
                retVal.Add(field.InternalName, schemaXml);

                return js.Serialize(retVal);
            }
            catch (Exception ex)
            {
                OnVerboseNotify($"Error getting schema for {fieldName} - {ex}");
                return string.Empty;
            }
        }

        public void GetFieldCreator(ClientContext ctx, string fieldName, AppManifestBase manifest)
        {
            try
            {
                var existingFieldCreators = manifest.Fields;

                existingFieldCreators = existingFieldCreators ?? new Dictionary<string, string>();

                var field = GetField(ctx, fieldName);
                if (field != null)
                {
                    OnVerboseNotify($"Got field creation information for {fieldName}");
                    var schemaXml = FieldTokenizer.DoTokenSubstitutions(ctx, field);
                    existingFieldCreators[field.InternalName] = schemaXml;
                }
                else
                {
                    OnVerboseNotify($"No information found for {fieldName}");
                }

                manifest.Fields = existingFieldCreators;
            }
            catch (Exception ex)
            {
                OnVerboseNotify($"Error getting schema for {fieldName} - {ex}");
            }
        }

        private Field GetField(ClientContext ctx, string fieldName)
        {
            var field = ctx.Site.RootWeb.Fields.GetByInternalNameOrTitle(fieldName);
            try
            {
                ctx.Load(field, f => f.InternalName, f => f.SchemaXml, f => f.TypeAsString);
                ctx.ExecuteQueryRetry();
            }
            catch (Exception ex)
            {
                OnVerboseNotify($"Error trying to get field from SharePoint {fieldName}. Error is: {ex.Message}");
                return null;
            }
            return field;
        }
    }
}