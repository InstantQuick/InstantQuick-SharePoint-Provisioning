using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;

namespace SharePointUtility
{
    public static class ListUtility
    {
        public static List<string> GetAllListTitles(ClientContext clientContext)
        {
            var retVal = new List<string>();
            clientContext.Load(clientContext.Web.Lists,
                l => l.Include
                    (list => list.Title));

            clientContext.ExecuteQueryRetry();

            foreach (var list in clientContext.Web.Lists)
            {
                retVal.Add(list.Title);
            }

            return retVal;
        }

        public static Dictionary<string, ContentType> GetAllContentTypesFromGroup(ClientContext clientContext,
            string group)
        {
            var retval = new Dictionary<string, ContentType>();
            //Get the available content types
            //These are needed for binding
            clientContext.Load(clientContext.Web.ContentTypes,
                c => c.Include
                    (contentType => contentType.Name).Where
                    (theCtype => theCtype.Group == group));

            clientContext.ExecuteQueryRetry();

            foreach (var contentType in clientContext.Web.ContentTypes)
            {
                retval.Add(contentType.Name, contentType);
            }

            return retval;
        }

        public static List CreateList(string title, string description, Guid templateFeatureId, int templateType,
            ClientContext clientContext)
        {
            var info = new ListCreationInformation
            {
                Title = title,
                TemplateFeatureId = templateFeatureId,
                TemplateType = templateType,
                Description = description,
                Url = "Lists/" + title
            };


            var list = clientContext.Web.Lists.Add(info);

            //Get the list's current content types
            clientContext.Load(list, l => l.Id, l => l.DefaultViewUrl, l => l.RootFolder);
            clientContext.Load(list.RootFolder, f => f.Properties);
            return list;
        }

        public static void ReplaceDefaultContentType(List list, string contentType,
            Dictionary<string, ContentType> existingTypes, ClientContext clientContext)
        {
            list.ContentTypes.AddExistingContentType(existingTypes[contentType]);
            list.ContentTypes[0].DeleteObject();

            clientContext.Load(list.Fields,
                f => f.Include
                    (field => field.SchemaXml, field => field.InternalName));

            clientContext.Load(list.Views,
                v => v.Include
                    (view => view.Id, view => view.ViewFields, view => view.Title));
        }

        //TODO: Refactor duplicate methods in IQAppData
        public static string SetXmlAttributeValue(this string xml, string attribute, string value)
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

        public static void HideFieldOnAllForms(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml =
                field.SchemaXml.SetXmlAttributeValue("ShowInDisplayForm", "FALSE")
                    .SetXmlAttributeValue("ShowInEditForm", "FALSE")
                    .SetXmlAttributeValue("ShowInNewForm", "FALSE");
            field.Update();
        }

        public static void HideFieldOnEditForm(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            field.SchemaXml = field.SchemaXml.SetXmlAttributeValue("ShowInEditForm", "FALSE");
            field.Update();
        }

        public static void IndexField(List list, string fieldName)
        {
            var field = GetFieldFromFieldsByName(list.Fields, fieldName);
            if (field == null) return; //TODO: Log.
            field.SchemaXml = field.SchemaXml.SetXmlAttributeValue("Indexed", "TRUE");
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

        public static ListItem FetchSingleItem(ClientContext clientContext, string listTitle, string filterField,
            string filterFieldType, string filterValue)
        {
            try
            {
                var items = FetchItems(clientContext, listTitle, filterField, filterFieldType, filterValue);
                if (items.Count > 0)
                {
                    return items[0];
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        public static ListItemCollection FetchItems(ClientContext clientContext, string listTitle, string filterField,
            string filterFieldType, string filterValue)
        {
            try
            {
                var list = clientContext.Web.Lists.GetByTitle(listTitle);

                var view =
                    $@"<View><ViewFields/>
  <Query>
  <Where>
      <Eq>
        <FieldRef Name='{filterField}' />
        <Value Type='{filterFieldType}'>{filterValue}</Value>
      </Eq>
    </Where>
  </Query>
</View>";

                var camlQuery = new CamlQuery {ViewXml = view};

                var items = list.GetItems(camlQuery);
                clientContext.Load(items);
                clientContext.ExecuteQueryRetry();
                return items;
            }
            catch
            {
                return null;
            }
        }
    }
}