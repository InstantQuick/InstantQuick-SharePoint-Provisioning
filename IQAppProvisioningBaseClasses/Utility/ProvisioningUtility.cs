using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;
using File = System.IO.File;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public static class Utility
    {
        //TODO: Refactor duplicate methods in ResourceServer.Data.ListUtility
        public static string SetXmlAttribute(this string xml, string attribute, string value)
        {
            XDocument document;
            using (var s = new StringReader(xml))
            {
                document = XDocument.Load(s);
            }

            var element = document.Root;
            element?.SetAttributeValue(attribute, value);

            return document.ToString(SaveOptions.DisableFormatting);
        }

        public static string RemoveXmlAttribute(this string xml, string attribute)
        {
            XDocument document;
            using (var s = new StringReader(xml))
            {
                document = XDocument.Load(s);
            }

            var element = document.Root;
            var attr = element?.Attribute(attribute);
            attr?.Remove();

            return document.ToString(SaveOptions.DisableFormatting);
        }

        public static string GetXmlAttribute(this string xml, string attribute)
        {
            XDocument document;
            using (var s = new StringReader(xml))
            {
                document = XDocument.Load(s);
            }

            var element = document.Root;
            var attr = element?.Attribute(attribute);
            return attr?.Value;
        }

        public static void AttachListToLookup(ClientContext ctx, Field lookupField, List list)
        {
            if (!list.IsPropertyAvailable("ParentWeb") || !list.IsPropertyAvailable("Id") ||
                !lookupField.IsPropertyAvailable("SchemaXml"))
            {
                ctx.Load(lookupField);
                ctx.Load(list.ParentWeb, l => l.Id);
                ctx.Load(list, l => l.Id);
                ctx.ExecuteQueryRetry();
            }
            var lookup = ctx.CastTo<FieldLookup>(lookupField);
            lookup.SchemaXml =
                lookup.SchemaXml
                    .SetXmlAttribute("List", list.Id.ToString())
                    .SetXmlAttribute("WebId", list.ParentWeb.Id.ToString());
            lookup.LookupList = list.Id.ToString();
            lookup.LookupWebId = list.ParentWeb.Id;
            lookupField.UpdateAndPushChanges(true);
            ctx.ExecuteQueryRetry();
        }

        public static void SetTitleFieldDisplayName(List list, string displayName)
        {
            var field = list.Fields.GetByInternalNameOrTitle("LinkTitle");
            field.Title = displayName;
            field.Update();
            TryUpdateField(field);
            field = list.Fields.GetByInternalNameOrTitle("LinkTitleNoMenu");
            field.Title = displayName;
            field.Update();
            TryUpdateField(field);
            field = list.Fields.GetByInternalNameOrTitle("Title");
            field.Title = displayName;
            field.Update();
            TryUpdateField(field);
        }

        private static void TryUpdateField(Field field)
        {
            try
            {
                field.Context.ExecuteQueryRetry();
            }
            catch
            {
                // ignored
            }
        }

        public static void SetFieldDisplayName(List list, string nameOrId, string displayName)
        {
            Guid id;
            var field = !Guid.TryParse(nameOrId, out id)
                ? list.Fields.GetByInternalNameOrTitle(nameOrId)
                : list.Fields.GetById(id);
            field.Title = displayName;
            field.Update();
        }

        public static void HideFieldOnAllForms(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml =
                field.SchemaXml.SetXmlAttribute("ShowInDisplayForm", "FALSE")
                    .SetXmlAttribute("ShowInEditForm", "FALSE")
                    .SetXmlAttribute("ShowInNewForm", "FALSE");
            field.Update();
        }

        public static void HideFieldOnEditForm(List list, string fieldName)
        {
            try
            {
                var field = GetFieldFromFieldsByName(list.Fields, fieldName);
                field.SchemaXml = field.SchemaXml.SetXmlAttribute("ShowInEditForm", "FALSE");
                field.Update();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error hiding field on edit form " + list + " | " + fieldName + " | " + ex);
                throw;
            }
        }

        public static void ShowOnDisplayFormOnly(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml =
                field.SchemaXml.SetXmlAttribute("ShowInDisplayForm", "TRUE")
                    .SetXmlAttribute("ShowInEditForm", "FALSE")
                    .SetXmlAttribute("ShowInNewForm", "FALSE");
            field.Update();
        }

        public static void HideField(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml = field.SchemaXml.SetXmlAttribute("Hidden", "TRUE");
            field.Update();
        }

        public static void IndexField(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml = field.SchemaXml.SetXmlAttribute("Indexed", "TRUE");
            field.Update();
        }

        public static void EnforceUniqueField(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml = field.SchemaXml.SetXmlAttribute("EnforceUniqueValues", "TRUE");
            field.SchemaXml = field.SchemaXml.SetXmlAttribute("Indexed", "TRUE");
            field.Update();
        }

        public static void RequireField(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.Required = true;
            field.Update();
        }

        public static Field GetFieldFromFieldsByName(FieldCollection fields, string fieldName)
        {
            foreach (var field in fields)
            {
                if (field.InternalName == fieldName) return field;
            }
            return null;
        }

        public static byte[] GetFile(string path, bool isBinary, string baseFilePath)
        {
            if (!baseFilePath.EndsWith(@"\")) baseFilePath = baseFilePath + @"\";
            return File.ReadAllBytes(baseFilePath + path);
        }

        public static byte[] GetFile(string key, bool isBinary, Assembly assembly)
        {
            if (assembly == null) return null;
            if (isBinary)
            {
                var binaryReader = new BinaryReader(assembly.GetManifestResourceStream(key));
                return ReadFully(binaryReader.BaseStream);
            }
            var streamReader = new StreamReader(assembly.GetManifestResourceStream(key));
            return ReadFully(streamReader.BaseStream);
        }

        public static string GetFile(string key, Assembly assembly)
        {
            if (assembly == null) return null;
            var streamReader = new StreamReader(assembly.GetManifestResourceStream(key));
            return Encoding.UTF8.GetString(ReadFully(streamReader.BaseStream));
        }

        private static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16*1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}