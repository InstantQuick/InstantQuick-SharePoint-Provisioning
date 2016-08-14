using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IQAppProvisioningBaseClasses.Events;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ContentTypeManager : ProvisioningManagerBase
    {
        private readonly List<string> _createdContentTypes = new List<string>();
        private readonly Dictionary<string, ContentType> _existingContentTypes = new Dictionary<string, ContentType>();

        //The site columns needed for the new content types
        private readonly Dictionary<string, Field> _siteFields = new Dictionary<string, Field>();
        private ClientContext _ctx;
        private Web _targetWeb;
        public virtual Dictionary<string, ContentTypeCreator> Creators { get; set; }
        public virtual List<string> OrderedDeletionList { get; } = new List<string>();

        public void CreateAll(ClientContext ctx)
        {
            _ctx = ctx;
            //Install to rootweb if not a hostweb
            _targetWeb = _ctx.Web.AppInstanceId == default(Guid) ? _ctx.Site.RootWeb : _ctx.Web;
            if (Creators != null && Creators.Count > 0)
            {
                try
                {
                    GetExistingFieldsAndContentTypes();
                    var parentTypes = GetParentContentTypes();
                    CreateContentTypes(parentTypes);
                    AddFields();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error creating content types at " + _targetWeb + " | " + ex);
                    throw;
                }
            }
        }

        public void DeleteAll(ClientContext ctx)
        {
            if (Creators != null && Creators.Count > 0)
            {
                _ctx = ctx;
                //Install to rootweb if not a hostweb
                _targetWeb = _ctx.Web.AppInstanceId == default(Guid) ? _ctx.Site.RootWeb : _ctx.Web;

                _ctx.Load(_targetWeb.ContentTypes,
                    c => c.Include(contentType => contentType.Name));

                _ctx.ExecuteQueryRetry();

                var deletionList = new Dictionary<string, ContentType>();

                foreach (var item in _targetWeb.ContentTypes)
                {
                    if (Creators.ContainsKey(item.Name))
                    {
                        Creators[item.Name].ContentType = item;

                        var id = Creators[item.Name].Id;
                        if (!string.IsNullOrEmpty(id))
                        {
                            deletionList[id] = item;
                        }
                    }
                }

                var sortedList = deletionList.OrderByDescending(kvp => kvp.Key).ToDictionary(c => c.Key, c => c.Value);

                foreach (var sortedListKey in sortedList.Keys)
                {
                    deletionList[sortedListKey].DeleteObject();
                }

                _ctx.ExecuteQueryRetry();
            }
        }

        private void GetExistingFieldsAndContentTypes()
        {
            foreach (var item in Creators.Values)
            {
                if (item.Fields != null)
                {
                    foreach (var fieldName in item.Fields)
                    {
                        if (!_siteFields.Keys.Contains(fieldName))
                        {
                            _siteFields[fieldName] = null;

                            //Should not use RootWeb here in case some doofus created a conflict elsewhere
                            _siteFields[fieldName] = _ctx.Web.AvailableFields.GetByInternalNameOrTitle(fieldName);
                        }
                    }
                }
            }

            //Should not use RootWeb here in case some doofus created a conflict elsewhere
            //Get all of the content types. Initialize the Name property
            _ctx.Load(_ctx.Web.AvailableContentTypes,
                c => c.Include
                    (contentType => contentType.Name, contentType => contentType.FieldLinks));

            _ctx.ExecuteQueryRetry();

            foreach (var contentType in _ctx.Web.AvailableContentTypes)
            {
                _existingContentTypes[contentType.Name] = contentType;
            }
        }

        private Dictionary<string, ContentType> GetParentContentTypes()
        {
            //Make a list of unique parent content types
            var parentTypes = new Dictionary<string, ContentType>();
            foreach (var item in Creators.Values)
            {
                if (!parentTypes.Keys.Contains(item.ParentContentTypeName))
                {
                    parentTypes[item.ParentContentTypeName] = null;
                }
            }

            //Associate the loaded instances with the list items
            foreach (var contentType in _ctx.Web.AvailableContentTypes)
            {
                if (parentTypes.ContainsKey(contentType.Name))
                {
                    parentTypes[contentType.Name] = contentType;
                }
            }
            return parentTypes;
        }

        private void CreateContentTypes(Dictionary<string, ContentType> parentTypes)
        {
            var i = 0;

            var sortedCreators = new List<KeyValuePair<string, ContentTypeCreator>>();

            var missingIds = false;
            foreach (var c in Creators.Values)
            {
                if (c.Id == null)
                {
                    missingIds = true;
                }
            }

            var sortingCreators = Creators.ToList();

            //Microsoft.SharePoint.Client.ServerException: parameters.Id, parameters.ParentContentType cannot be used together. Please only use one of them.
            if (_ctx.ServerVersion >= Version.Parse("15.0.4569.1509") && !missingIds)
            {
                //By definition, sorting by the content type ID puts the parents before the childres 
                sortingCreators.Sort((p1, p2) => string.Compare(p1.Value.Id, p2.Value.Id, StringComparison.Ordinal));
                sortedCreators = sortingCreators;
            }
            else
            {
                foreach (var c in sortingCreators)
                {
                    if (!sortedCreators.Contains(c))
                    {
                        sortedCreators.Add(c);
                    }
                    if (c.Value.ParentContentTypeName != null && Creators.Keys.Contains(c.Value.ParentContentTypeName))
                    {
                        var parentKvp = new KeyValuePair<string, ContentTypeCreator>(c.Value.ParentContentTypeName,
                            Creators[c.Value.ParentContentTypeName]);
                        if (!sortedCreators.Contains(parentKvp))
                        {
                            sortedCreators.Insert(sortedCreators.IndexOf(c), parentKvp);
                        }
                    }
                }
            }

            //Create the new types
            foreach (var creator in sortedCreators)
            {
                if (!_existingContentTypes.Keys.Contains(creator.Key))
                {
                    i++;
                    var typeInfo = new ContentTypeCreationInformation {Name = creator.Key};

                    //Microsoft.SharePoint.Client.ServerException: parameters.Id, parameters.ParentContentType cannot be used together. Please only use one of them.
                    if (!string.IsNullOrEmpty(creator.Value.Id) && _ctx.ServerVersion >= Version.Parse("15.0.4569.1509"))
                    {
                        typeInfo.Id = creator.Value.Id;
                    }
                    else
                    {
                        typeInfo.ParentContentType = parentTypes[creator.Value.ParentContentTypeName];

                        //If the parent type didn't exist before, it might now in one of the creators
                        //TODO: Once it's all SP1 CSOM this won't be needed. Always use the ID.
                        if (typeInfo.ParentContentType == null &&
                            Creators.Keys.Contains(creator.Value.ParentContentTypeName) &&
                            Creators[creator.Value.ParentContentTypeName].ContentType != null)
                        {
                            typeInfo.ParentContentType = Creators[creator.Value.ParentContentTypeName].ContentType;
                        }
                    }

                    typeInfo.Description = creator.Value.Description;
                    typeInfo.Group = creator.Value.Group;

                    //Always create in root web
                    Creators[creator.Key].ContentType = _targetWeb.ContentTypes.Add(typeInfo);
                    _ctx.Load(Creators[creator.Key].ContentType, c => c.Name, c => c.FieldLinks);

                    OnNotify(ProvisioningNotificationLevels.Verbose, "Creating content type " + creator.Key);

                    //Workaround for 'The request uses too many resources bug', do 5 at a time
                    if (i%5 == 0)
                    {
                        _ctx.ExecuteQueryRetry();
                    }
                }
                else
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose,
                        "Creating content " + creator.Key + " already exists. Skipping creation");
                    creator.Value.ContentType = _existingContentTypes[creator.Key];
                }
                _createdContentTypes.Add(creator.Key);
            }
            _ctx.ExecuteQueryRetry();
        }

        private void AddFields()
        {
            //Add the fields to each content type
            foreach (var creatorkey in Creators.Keys)
            {
                if (_createdContentTypes.Contains(creatorkey))
                {
                    OnNotify(ProvisioningNotificationLevels.Verbose, "Processing fields for content type " + creatorkey);
                    var creator = Creators[creatorkey];
                    if (creator.Fields != null)
                    {
                        var fieldsToAdd = new List<string>(creator.Fields);
                        foreach (var fl in creator.ContentType.FieldLinks)
                        {
                            if (fieldsToAdd.Contains(fl.Name))
                            {
                                //Hack attempt to force update of lists with this type
                                fl.Required = fl.Required;
                                fieldsToAdd.Remove(fl.Name);
                            }
                            //HACK: SharePoint's event type and calendar list are shit!
                            if (fl.Name == "EventDate" && fieldsToAdd.Contains("StartDate"))
                            {
                                fieldsToAdd.Remove("StartDate");
                            }
                            if (fl.Name == "Description" && fieldsToAdd.Contains("Comments"))
                            {
                                fieldsToAdd.Remove("Comments");
                            }
                        }
                        foreach (var fieldKey in fieldsToAdd)
                        {
                            var info = new FieldLinkCreationInformation {Field = _siteFields[fieldKey]};
                            creator.ContentType.FieldLinks.Add(info);
                        }

                        //Must save at this point to be able to remove undesired fields as the next step
                        creator.ContentType.Name = creatorkey;
                        creator.ContentType.Update(true);
                        _ctx.ExecuteQueryRetry();

                        if (creator.RemoveFields != null && creator.RemoveFields.Count > 0)
                        {
                            //Refresh the FieldLinks because some nay have been added
                            _ctx.Load(creator.ContentType, c => c.Name, c => c.FieldLinks);
                            _ctx.ExecuteQueryRetry();

                            foreach (var fieldToRemove in creator.RemoveFields)
                            {
                                foreach (var fl in creator.ContentType.FieldLinks)
                                {
                                    if (fl.Name == fieldToRemove)
                                    {
                                        fl.DeleteObject();
                                        break;
                                    }
                                }
                            }
                        }
                        if (creator.OrderedFields != null && creator.OrderedFields.Count > 0)
                        {
                            var reorder = creator.OrderedFields.ToArray();
                            creator.ContentType.FieldLinks.Reorder(reorder);
                        }
                        creator.ContentType.Update(true);
                    }
                }
                _ctx.ExecuteQueryRetry();
            }
        }
    }
}