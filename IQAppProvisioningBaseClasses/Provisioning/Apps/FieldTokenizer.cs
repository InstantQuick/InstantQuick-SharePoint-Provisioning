using System;
using System.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using System.Collections.Generic;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class FieldTokenizer
    {
        public static string DoTokenSubstitutions(ClientContext ctx, Field field)
        {
            var schemaXml = field.SchemaXml;
            var newXml = SubstituteGroupTokens(ctx, schemaXml);
            newXml = TokenizeLookupField(ctx, newXml);
            newXml = TokenizeTaxonomyField(ctx, field, newXml);
            return newXml;
        }

        public static string DoTokenReplacement(ClientContext ctx, string schemaXml)
        {
            var newXml = ReplaceGroupTokens(ctx, schemaXml);
            newXml = ReplaceListTokens(ctx, newXml);
            newXml = ReplaceTaxonomyTokens(ctx, newXml);
            return newXml;
        }

        private static string SubstituteGroupTokens(ClientContext ctx, string schemaXml)
        {
            if (!schemaXml.Contains("UserSelectionScope"))
            {
                return schemaXml;
            }
            var groupId = schemaXml.GetXmlAttribute("UserSelectionScope");
            if (groupId == null)
            {
                return schemaXml;
            }

            int id;
            if (int.TryParse(groupId, out id) && id != 0)
            {
                var group = ctx.Web.SiteGroups.GetById(id);
                ctx.Load(group, g => g.Title);
                ctx.Load(ctx.Web.AssociatedMemberGroup, g => g.Id);
                ctx.Load(ctx.Web.AssociatedOwnerGroup, g => g.Id);
                ctx.Load(ctx.Web.AssociatedVisitorGroup, g => g.Id);
                ctx.ExecuteQueryRetry();

                var tokenTitle = group.Title;

                if (id == ctx.Web.AssociatedMemberGroup.Id)
                {
                    tokenTitle = "AssociatedMemberGroup";
                }
                if (id == ctx.Web.AssociatedOwnerGroup.Id)
                {
                    tokenTitle = "AssociatedOwnerGroup";
                }
                if (id == ctx.Web.AssociatedVisitorGroup.Id)
                {
                    tokenTitle = "AssociatedVisitorGroup";
                }

                schemaXml = schemaXml.SetXmlAttribute("UserSelectionScope", tokenTitle);
            }
            return schemaXml;
        }

        private static string TokenizeLookupField(ClientContext ctx, string schemaXml)
        {
            var retval = schemaXml;

            var fieldType = retval.GetXmlAttribute("Type");
            if (fieldType == "Lookup")
            {
                List lookupTarget = null;
                var listIdOrUrl = retval.GetXmlAttribute("List");
                if (listIdOrUrl != null)
                {
                    Guid listGuid;
                    if (Guid.TryParse(listIdOrUrl, out listGuid))
                    {
                        lookupTarget = ctx.Web.Lists.GetById(listGuid);
                        ctx.Load(lookupTarget, l => l.Title);
                        ctx.ExecuteQueryRetry();
                    }
                    else if (listIdOrUrl.Contains("/"))
                    {
                        if (!listIdOrUrl.StartsWith("/"))
                        {
                            listIdOrUrl = "/" + listIdOrUrl;
                        }
                        var baseUrl = ctx.Web.ServerRelativeUrl == "/" ? "" : ctx.Web.ServerRelativeUrl;

                        //Get list is new since CSOM v15.0.4701.1001
                        if (ctx.ServerVersion >= Version.Parse("15.0.4701.1001"))
                        {
                            lookupTarget = ctx.Web.GetList(baseUrl + listIdOrUrl);
                            ctx.Load(lookupTarget, l => l.Title);
                            ctx.ExecuteQueryRetry();
                        }
                        else
                        {
                            var lists = ctx.Web.Lists;
                            ctx.Load(lists, ls => ls.Include(l => l.DefaultViewUrl, l => l.Title));
                            ctx.ExecuteQueryRetry();
                            foreach (var l in lists)
                            {
                                if (l.DefaultViewUrl.ToLower().Contains(listIdOrUrl.ToLower()))
                                {
                                    lookupTarget = l;
                                    break;
                                }
                            }
                        }
                    }
                    if (lookupTarget != null && lookupTarget.IsPropertyAvailable("Title"))
                    {
                        retval = retval.SetXmlAttribute("List", "{@ListId:" + lookupTarget.Title + "}");
                    }
                }
            }

            return retval;
        }

        private static string TokenizeTaxonomyField(ClientContext ctx, Field field, string schemaXml)
        {
            var fieldType = schemaXml.GetXmlAttribute("Type");
            if (!fieldType.StartsWith("TaxonomyField")) return schemaXml;

            schemaXml = schemaXml.RemoveXmlAttribute("List");
            schemaXml = schemaXml.RemoveXmlAttribute("WebId");
            schemaXml = schemaXml.RemoveXmlAttribute("SourceID");
            schemaXml = schemaXml.RemoveXmlAttribute("Version");
            //Default Value

            var taxonomyField = ctx.CastTo<TaxonomyField>(field);
            ctx.Load(taxonomyField);
            ctx.ExecuteQueryRetry();

            var defaultValue = taxonomyField.DefaultValue;
            if (defaultValue != null)
            {
                var defaultValueToken = $"{{@DefaultValue:{defaultValue.GetInnerText(";#", "|")}}}";
                //schemaXml = schemaXml.Replace(defaultValue, defaultValueToken);

                //There is no good way to set the default value in CSOM for field when you are creating it
                //because it requires the WssId of the term, which in most cases will not exist in the site's HiddenTaxonomyList
                //If you need to do this, add a dummy item to a list that contains this field with the value set
                //get the WssId, update the property, and delete the dummy record as a post deployment step
                //or do it manually in the UI
                schemaXml = schemaXml.Replace(defaultValue, "");
            }

            var sspId = taxonomyField.SspId.ToString();
            schemaXml = schemaXml.Replace(sspId, "{@SspId}");

            if (taxonomyField.TermSetId != default(Guid) ||
                taxonomyField.AnchorId != default(Guid))
            {
                var taxonomySession = TaxonomySession.GetTaxonomySession(ctx);
                var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
                TermSet termSet = null;
                Term anchorTerm = null;

                if (taxonomyField.TermSetId != default(Guid))
                {
                    termSet = termStore.GetTermSet(taxonomyField.TermSetId); 
                    ctx.Load(termSet, ts => ts.Name);    
                }
                if (taxonomyField.AnchorId != default(Guid))
                {
                    anchorTerm = termStore.GetTerm(taxonomyField.AnchorId);
                    ctx.Load(anchorTerm, t => t.Name);
                }
                try
                {
                    ctx.ExecuteQueryRetry();
                }
                catch
                {
                    //ignore
                }

                if (termSet != null)
                {
                    schemaXml = schemaXml.Replace(taxonomyField.TermSetId.ToString(), $"{{@TermSet:{termSet.Name}}}");
                }
                if (anchorTerm != null)
                {
                    schemaXml = schemaXml.Replace(taxonomyField.AnchorId.ToString(), $"{{@AnchorTermId:{anchorTerm.Name}}}");
                }
            }

            return schemaXml;
        }

        private static string ReplaceGroupTokens(ClientContext ctx, string schemaXml)
        {
            if (!schemaXml.Contains("UserSelectionScope"))
            {
                return schemaXml;
            }
            if (ctx.Web.AppInstanceId != default(Guid))
            {
                return schemaXml.RemoveXmlAttribute("UserSelectionScope");
            }
            var groupName = schemaXml.GetXmlAttribute("UserSelectionScope");
            if (groupName == null || groupName == "0")
            {
                return schemaXml;
            }

            Group group;
            if (groupName == "AssociatedMemberGroup")
            {
                group = ctx.Web.AssociatedMemberGroup;
            }
            else if (groupName == "AssociatedOwnerGroup")
            {
                group = ctx.Web.AssociatedOwnerGroup;
            }
            else if (groupName == "AssociatedVisitorGroup")
            {
                group = ctx.Web.AssociatedVisitorGroup;
            }
            else
            {
                group = ctx.Web.SiteGroups.GetByName(groupName);
            }
            ctx.Load(group, g => g.Id);
            ctx.ExecuteQueryRetry();

            schemaXml = schemaXml.SetXmlAttribute("UserSelectionScope", group.Id.ToString());

            return schemaXml;
        }

        private static string ReplaceListTokens(ClientContext ctx, string schemaXml)
        {
            var retval = schemaXml;

            var listTitle = retval.GetInnerText("{@ListId:", "}", true);
            if (!string.IsNullOrEmpty(listTitle))
            {
                var list = ctx.Web.Lists.GetByTitle(listTitle);
                ctx.Load(list, l => l.Id);

                try
                {
                    ctx.ExecuteQueryRetry();
                }
                catch
                {
                    //Ignore. In some versions of CSOM the list not existing will give a runtime error
                    //In others it just doesn't load the object and we can check property availabilty
                }
                if (list.IsPropertyAvailable("Id"))
                {
                    retval = retval.SetXmlAttribute("List", "{" + list.Id + "}");
                }
            }

            return retval;
        }

        private static string ReplaceTaxonomyTokens(ClientContext ctx, string schemaXml)
        {
            var tokens = new List<string>()
            {
                "{@DefaultValue:",
                "{@SspId}",
                "{@TermSet:",
                "{@AnchorTermId:"
            };

            var foundTokens = tokens.Where(t => schemaXml.Contains(t)).ToList();
            if (foundTokens.Count == 0) return schemaXml;

            var taxonomySession = TaxonomySession.GetTaxonomySession(ctx);
            var termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            ctx.Load(termStore, ts => ts.Id);
            ctx.ExecuteQuery();

            TermCollection terms = null;

            schemaXml = schemaXml.Replace("{@SspId}", termStore.Id.ToString());
            var termSetName = schemaXml.GetInnerText("{@TermSet:", "}");
            var termSets = termStore.GetTermSetsByName(termSetName, (int) ctx.Web.Language);
            ctx.Load(termSets, ts => ts.Include(t => t.Id));
            ctx.ExecuteQueryRetry();

            if (termSets.Count == 0)
            {
                throw new InvalidOperationException($"Unable to find term set {termSetName}.");
            }

            schemaXml = schemaXml.Replace($"{{@TermSet:{termSetName}}}", termSets[0].Id.ToString());
            terms = termSets[0].GetAllTerms();
            ctx.Load(terms);
            ctx.ExecuteQueryRetry();
            if (foundTokens.Contains("{@AnchorTermId:"))
            {
                var anchorTermName = schemaXml.GetInnerText("{@AnchorTermId:", "}");
                var foundAnchorTerm = terms.FirstOrDefault(t => t.Name == anchorTermName);
                ctx.ExecuteQuery();
                if (foundAnchorTerm != null)
                {
                    schemaXml = schemaXml.Replace($"{{@AnchorTermId:{anchorTermName}}}", foundAnchorTerm.Id.ToString());
                }
            }

            if (foundTokens.Contains("{@DefaultValue:"))
            {
                var defaultValueTermName = schemaXml.GetInnerText("{@DefaultValue:", "}");
                var foundDefaultTerm = terms.FirstOrDefault(t => t.Name == defaultValueTermName);
                ctx.ExecuteQueryRetry();
                if (foundDefaultTerm != null)
                {
                    schemaXml = schemaXml.Replace($"{{@DefaultValue:{defaultValueTermName}}}",
                        $"-1;#{defaultValueTermName}|{foundDefaultTerm.Id.ToString()}");
                }
            }

            return schemaXml;
        }
    }
}