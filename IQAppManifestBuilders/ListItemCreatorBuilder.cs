using System;
using System.Collections.Generic;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using static IQAppProvisioningBaseClasses.Utility.Tokenizer;

namespace IQAppManifestBuilders
{
    public class ListItemCreatorBuilder
    {
        public ListItemCreatorBuilder(ClientContext ctx, Web web, List list)
        {
            ClientContext = ctx;
            Web = web;
            List = list;
        }

        public ListItemCreatorBuilder(ClientContext ctx, Web web, List list, TaxonomySession taxonomySession, TermStore termStore)
        {
            ClientContext = ctx;
            Web = web;
            TaxonomySession = taxonomySession;
            TermStore = termStore;
        }

        public ClientContext ClientContext { get; set; }
        public Web Web { get; set; }
        public List List { get; set; }
        public TaxonomySession TaxonomySession { get; set; }
        public TermStore TermStore { get; set; }

        private readonly Dictionary<string, TaxonomyField> _taxonomyFields = new Dictionary<string, TaxonomyField>();

        private readonly Dictionary<Guid, string> _termSetNames = new Dictionary<Guid, string>();

        public ListItemCreator GetListItemCreator(ListItem listItem)
        {
            var itemCreator = new ListItemCreator() { FieldValues = new List<ListItemFieldValue>() };
            List.EnsureProperties(l => l.Fields);

            //{67df98f4-9dec-48ff-a553-29bece9c5bf4} is Attachments
            var attachmentsFieldId = Guid.Parse("{67df98f4-9dec-48ff-a553-29bece9c5bf4}");

            foreach (var field in List.Fields)
            {
                if (listItem.FieldValuesForEdit.FieldValues.ContainsKey(field.InternalName) &&
                    !string.IsNullOrEmpty(listItem.FieldValuesForEdit[field.InternalName]) &&
                    ((!field.Hidden && 
                    !field.ReadOnlyField && 
                    field.Id != attachmentsFieldId) ||
                    field.InternalName == "ContentTypeId") ||
                    field.InternalName == "PublishingPageLayout")
                {
                    try
                    {
                        var fieldValuePair = new ListItemFieldValue()
                        {
                            FieldName = field.InternalName,
                            Value = TokenizeUrls(Web, listItem.FieldValuesForEdit[field.InternalName]),
                            FieldType = field.TypeAsString ?? String.Empty
                        };
                        if (fieldValuePair.FieldType == "DateTime" && fieldValuePair.Value != null)
                        {
                            fieldValuePair.Value =
                                (new DateTimeOffset(DateTime.Parse(fieldValuePair.Value))).UtcDateTime.ToString(
                                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                        }
                        if (fieldValuePair.FieldType.StartsWith("TaxonomyField"))
                        {
                            if (TaxonomySession == null)
                            {
                                TaxonomySession = TaxonomySession.GetTaxonomySession(ClientContext);
                                TermStore = TaxonomySession.GetDefaultSiteCollectionTermStore();
                            }
                            if (!_taxonomyFields.ContainsKey(fieldValuePair.FieldName))
                            {
                                _taxonomyFields[fieldValuePair.FieldName] =
                                    ClientContext.CastTo<TaxonomyField>(field);

                                ClientContext.Load(_taxonomyFields[fieldValuePair.FieldName], tx => tx.TermSetId);
                                ClientContext.ExecuteQueryRetry();
                            }
                            if (!_termSetNames.ContainsKey(
                                _taxonomyFields[fieldValuePair.FieldName].TermSetId))
                            {
                                var termSet =
                                    TermStore.GetTermSet(_taxonomyFields[fieldValuePair.FieldName].TermSetId);

                                ClientContext.Load(termSet, ts => ts.Name);
                                ClientContext.ExecuteQueryRetry();
                                _termSetNames[_taxonomyFields[fieldValuePair.FieldName].TermSetId] = termSet.Name;
                            }

                            fieldValuePair.Value =
                                $"{{@TermSet:{_termSetNames[_taxonomyFields[fieldValuePair.FieldName].TermSetId]}}}|{{@Terms:{fieldValuePair.Value}}} ";
                        }

                        itemCreator.FieldValues.Add(fieldValuePair);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            //Some catalog items don't have a content type (composed looks)
            //No idea why
            if (listItem.ContentType.ServerObjectIsNull != true)
            {
                //Actual content type name is required for versions of CSOM that don't support ContentTypeId
                //If the package also includes the ContentType the ID will not be the same between sites
                //Note this is a read-only field
                var contentTypeValuePair = new ListItemFieldValue()
                {
                    FieldName = "ContentType",
                    Value = listItem.ContentType.Name,
                    FieldType = "ContentType"
                };
                itemCreator.FieldValues.Add(contentTypeValuePair);
            }

            return itemCreator;
        }
    }
}
