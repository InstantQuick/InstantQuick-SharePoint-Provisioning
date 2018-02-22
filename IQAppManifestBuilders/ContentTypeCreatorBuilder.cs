using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using IQAppProvisioningBaseClasses.Provisioning;
using Microsoft.SharePoint.Client;

namespace IQAppManifestBuilders
{
    /// <summary>
    /// Creates a serialized depiction of a content type for recreation elsewhere.
    /// The creator will not include inherited fields except to note the order in which they should appear.
    /// This is the slowest creator in the stack because understanding the heirarchy of a single content type
    /// requires analysis of all it's ancestors.
    /// </summary>
    public class ContentTypeCreatorBuilder : CreatorBuilderBase
    {
        /// <summary>
        /// Emits a content type creator as a json string
        /// </summary>
        /// <param name="ctx">The client context</param>
        /// <param name="contentTypeName">The web. If null uses RootWeb</param>
        /// <param name="contentTypeName">The content type to read</param>
        /// <returns></returns>
        public string GetContentTypeCreator(ClientContext ctx, Web web, string contentTypeName)
        {
            var manifest = new AppManifestBase();
            GetContentTypeCreator(ctx, web, contentTypeName, manifest);
            if (manifest.ContentTypeCreators != null && manifest.ContentTypeCreators.ContainsKey(contentTypeName))
            {
                var js = new JavaScriptSerializer();
                return js.Serialize(manifest.ContentTypeCreators[contentTypeName]);
            }
            OnVerboseNotify("NO INFORMATION FOUND FOR " + contentTypeName);
            return string.Empty;
        }

        /// <summary>
        /// Adds a content type creator to a given manifest
        /// </summary>
        /// <param name="ctx">The client context</param>
        /// <param name="web">The web. RootWeb if null</param>
        /// <param name="contentTypeName">The web. If null uses RootWeb</param>
        /// <param name="manifest">The manifest to which the creator is to be added</param>
        public void GetContentTypeCreator(ClientContext ctx, Web web, string contentTypeName, AppManifestBase manifest)
        {
            if (manifest == null) return;

            web = web ?? ctx.Site.RootWeb;

            var existingContentTypeCreators = manifest.ContentTypeCreators;
            existingContentTypeCreators = existingContentTypeCreators ?? new Dictionary<string, ContentTypeCreator>();
            var contentTypeCreators = GetContentTypeCreatorFromSite(ctx, web, contentTypeName);
            if (contentTypeCreators == null)
            {
                OnVerboseNotify($"No information found for content type {contentTypeName}");
                return;
            }
            existingContentTypeCreators[contentTypeName] = contentTypeCreators[contentTypeName];
            manifest.ContentTypeCreators = existingContentTypeCreators;
            OnVerboseNotify($"Got content type creation information for {contentTypeName}");
        }

        private Dictionary<string, ContentTypeCreator> GetContentTypeCreatorFromSite(ClientContext ctx, Web web,
            string contentTypeName)
        {
            var contentTypes = web.ContentTypes;

            ctx.Load(contentTypes,
                c => c.Include
                    (contentType => contentType.Name,
                        contentType => contentType.Id,
                        contentType => contentType.Fields.Include(f => f.InternalName, f => f.FieldTypeKind, f => f.Hidden, f => f.Title),
                        contentType => contentType.FieldLinks.Include(fl => fl.Name),
                        contentType => contentType.Description,
                        contentType => contentType.Group,
                        contentType => contentType.Parent));

            ctx.ExecuteQueryRetry();

            var existingContentTypes = GetExistingContentTypesList(contentTypes);

            if (!existingContentTypes.ContainsKey(contentTypeName))
            {
                return null;
            }

            var retList = new Dictionary<string, ContentTypeCreator>
            {
                [contentTypeName] = existingContentTypes[contentTypeName]
            };
            return retList;
        }

        /// <summary>
        /// Populates a dictionary of existing content types in the site and their complete definitions
        /// </summary>
        /// <param name="contentTypes">The content types from the context's root web</param>
        /// <returns></returns>
        private Dictionary<string, ContentTypeCreator> GetExistingContentTypesList(ContentTypeCollection contentTypes)
        {
            var retList = new Dictionary<string, ContentTypeCreator>();
            foreach (var ctype in contentTypes)
            {
                if (!retList.ContainsKey(ctype.Name))
                {
                    var newCreator = new ContentTypeCreator
                    {
                        Id = ctype.Id.StringValue,
                        ParentContentTypeName = ctype.Parent.Name,
                        Group = ctype.Group,
                        Description = ctype.Description,
                        Fields = new List<string>(),
                        OrderedFields = new List<string>(),
                        RemoveFields = new List<string>()
                    };

                    retList.Add(ctype.Name, newCreator);
                    foreach (var field in ctype.Fields)
                    {
                        Guid possibleTaxonomyField = default(Guid);

                        //Skip the hidden taxonomy fields
                        if (field != null &&
                            (!field.InternalName.StartsWith("TaxCatchAll") &&
                             !(field.FieldTypeKind == FieldType.Note && field.Hidden &&
                               Guid.TryParseExact(field.InternalName, "N", out possibleTaxonomyField) &&
                               field.Title.Contains("_"))))
                        {
                            newCreator.Fields.Add(field.InternalName);
                        }
                    }
                    foreach (var fieldLink in ctype.FieldLinks)
                    {
                        var name = fieldLink.Name;
                        //Because SharePoint...
                        if (name == "EventDate") name = "StartDate";
                        if (newCreator.Fields.Contains(name))
                        {
                            newCreator.OrderedFields.Add(name);
                        }
                    }
                }
            }

            //Remove inherited fields and TaxonomyFields
            foreach (var contentTypeCreator in retList.Values)
            {
                if (!string.IsNullOrEmpty(contentTypeCreator.ParentContentTypeName) &&
                    retList.ContainsKey(contentTypeCreator.ParentContentTypeName))
                {
                    var ptype = retList[contentTypeCreator.ParentContentTypeName];
                    var parentFields = GetParentContentTypeFields(retList, ptype);
                    foreach (var field in parentFields)
                    {
                        if (contentTypeCreator.Fields.Contains(field))
                        {
                            contentTypeCreator.Fields.Remove(field);
                        }
                        else
                        {
                            contentTypeCreator.RemoveFields.Add(field);
                        }
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// First step of a recursive operation to read the fields from which a given type inherits
        /// </summary>
        /// <param name="contentTypes">Existing content types</param>
        /// <param name="ctype">The content type creator being built</param>
        /// <returns></returns>
        private List<string> GetParentContentTypeFields(Dictionary<string, ContentTypeCreator> contentTypes,
            ContentTypeCreator ctype)
        {
            var fields = new List<string>();
            return GetParentContentTypeFields(contentTypes, ctype, fields);
        }

        /// <summary>
        /// Recursive operation to read the fields from which a given type inherits
        /// </summary>
        /// <param name="contentTypes">Existing content types</param>
        /// <param name="ctype">The content type creator being built</param>
        /// <param name="fields">The ancestor fields</param>
        /// <returns></returns>
        private List<string> GetParentContentTypeFields(Dictionary<string, ContentTypeCreator> contentTypes,
            ContentTypeCreator ctype, List<string> fields)
        {
            foreach (var field in ctype.Fields)
            {
                if (!fields.Contains(field))
                {
                    fields.Add(field);
                }
            }
            if (!string.IsNullOrEmpty(ctype.ParentContentTypeName) &&
                contentTypes.ContainsKey(ctype.ParentContentTypeName) && ctype.Id != "0x")
            {
                fields = GetParentContentTypeFields(contentTypes, contentTypes[ctype.ParentContentTypeName], fields);
            }
            return fields;
        }
    }
}