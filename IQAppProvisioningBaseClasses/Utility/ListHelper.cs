using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.SharePoint.Client;

namespace SharePointUtility
{
    //Classes support an object that allows quick determination
    //of list title and content type by id
    public class ListInfo
    {
        public string ListId { get; set; }
        public string ListTitle { get; set; }
        public string ContentType { get; set; }
        public ListInfo() { }
        public ListInfo(string listId, string listTitle, string contentType)
        {
            ListId = listId;
            ListTitle = listTitle;
            ContentType = contentType;
        }
        public ListInfo(List list, string contentType)
        {
            ListId = list.Id.ToString();
            ListTitle = list.Title;
            ContentType = contentType;
        }
    }

    public class ListInfoCollection
    {
        public Dictionary<string, ListInfo> Lists { get; set; } = new Dictionary<string, ListInfo>();
    }

    public class ListHelper
    {
        public ListInfoCollection ById = new ListInfoCollection();
        //public ListInfoCollection ByTitle = new ListInfoCollection();

        //Refactor this coercion to lower. Crude force for predictable
        //behavior looking up from javascript.
        public void Add(ListInfo item)
        {
            item.ListId = item.ListId.ToLower();
            ById.Lists[item.ListId] = item;
        }

        public static string ToJson(ListHelper instance)
        {
            var js = new JavaScriptSerializer();
            return js.Serialize(instance);
        }

        public static ListHelper FromJson(string data)
        {
            var js = new JavaScriptSerializer();
            return js.Deserialize<ListHelper>(data);
        }

        public static ListHelper FromSettings(ClientContext ctx)
        {
            var js = new JavaScriptSerializer();

            var configItem = GetListIdsFromSettings(ctx);

            if (configItem.Count < 1) return new ListHelper();
            return js.Deserialize<ListHelper>(configItem[0]["Value"].ToString());
        }

        private static ListItemCollection GetListIdsFromSettings(ClientContext ctx)
        {
            var query = @"<View>
  <ViewFields>
    <FieldRef Name='Title' />
    <FieldRef Name='Value' />
  </ViewFields>
  <Query>
    <Where>
      <Eq>
        <FieldRef Name='Title' />
        <Value Type='Text'>ListIds</Value>
      </Eq>
    </Where>
  </Query>
</View>";

            var list = ctx.Web.Lists.GetByTitle("Settings");
            var view = new CamlQuery {ViewXml = query};
            var configItem = list.GetItems(view);
            ctx.Load(configItem);
            ctx.ExecuteQueryRetry();
            return configItem;
        }

        public static void ToSettings(ListHelper instance, ClientContext ctx)
        {
            var js = new JavaScriptSerializer();
            var existingItems = GetListIdsFromSettings(ctx);
            ListItem listIds;
            if (existingItems.Count < 1)
            {
                var list = ctx.Web.Lists.GetByTitle("Settings");
                listIds = list.AddItem(new ListItemCreationInformation());
                listIds["Title"] = "ListIds";
            }
            else
            {
                listIds = existingItems[0];
            }

            listIds["Value"] = js.Serialize(instance);
            listIds.Update();

            UpdateSettingsJs(ctx);
        }

        public static void UpdateSettingsJs(ClientContext ctx)
        {
            var query = @"<View>
  <ViewFields>
    <FieldRef Name='Title' />
    <FieldRef Name='Value' />
  </ViewFields>
  <Query></Query>
</View>";
            var view = new CamlQuery {ViewXml = query};

            try
            {
                var items = ctx.Web.Lists.GetByTitle("Settings").GetItems(view);
                ctx.Load(items);
                ctx.Load(ctx.Web, w => w.ServerRelativeUrl);
                ctx.ExecuteQueryRetry();

                var root = ctx.Web.RootFolder;
                root.Folders.Add("Scripts");

                try
                {
                    ctx.ExecuteQueryRetry();
                }
                catch
                {
                    // ignored
                }

                var script = "window.lps = {};";
                foreach (var item in items)
                {
                    script = script + $"lps.{item["Title"]} = '{item["Value"]}';";
                }
                var scriptFile = Encoding.UTF8.GetBytes(script);

                UploadFile(ctx, null, scriptFile, "/scripts/settings.js");
            }
            catch
            {
                // ignored
            }
        }

        //Refactor this coercion to lower. Crude force for predictable
        //behavior looking up from javascript.
        internal ListHelper AddListDataToHelper(ListInfo item, ClientContext ctx)
        {
            item.ListId = item.ListId.ToLower();
            var helper = FromSettings(ctx);
            Add(item);
            ToSettings(helper, ctx);
            return helper;
        }

        internal void AddList(List list, string contentType, ClientContext ctx)
        {
            var listInfo = new ListInfo
            {
                ListId = list.Id.ToString(),
                ListTitle = list.Title,
                ContentType = contentType
            };
            AddListDataToHelper(listInfo, ctx);
        }

        public static File UploadFile(ClientContext ctx, string list, byte[] file, string url)
        {
            Folder folder;

            var fileCreationInformation = new FileCreationInformation
            {
                Content = file,
                Overwrite = true
            };
            if (ctx.Web.ServerRelativeUrl != "/") url = ctx.Web.ServerRelativeUrl + url;
            fileCreationInformation.Url = url;

            if (!string.IsNullOrEmpty(list))
            {
                var library = ctx.Web.Lists.GetByTitle(list);
                folder = library.RootFolder;
            }
            else
            {
                folder = ctx.Web.RootFolder;
            }

            var uploadFile = folder.Files.Add(fileCreationInformation);

            ctx.ExecuteQueryRetry();

            return uploadFile;
        }

    }
}