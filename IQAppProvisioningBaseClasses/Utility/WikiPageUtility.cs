using System.Collections.Generic;

namespace IQAppProvisioningBaseClasses
{
    public class WikiPageUtility
    {
        public static List<string> GetStorageKeysFromWikiContent(string wikiContent)
        {
            //TODO: Change to Scrapy/HtmlAgility
            return wikiContent.GetInnerTextList("id=\"div_", "\"");
        }

        public static Dictionary<string, string> GetStorageKeyMappings(string page, List<string> storageKeys)
        {
            var storageKeyMappings = new Dictionary<string, string>();
            foreach (var storageKey in storageKeys)
            {
                var webPartId = WebPartUtility.LookForWebPartId(page, storageKey);
                if (webPartId != string.Empty)
                {
                    storageKeyMappings[webPartId] = storageKey;
                }
            }
            return storageKeyMappings;
        }

        public static string GetUpdatedWikiContentText(string wikiContent,
            Dictionary<string, string> originalStorageKeyMappings, Dictionary<string, string> newWebPartIdMappings)
        {
            var newWikiContent = wikiContent;

            //Original storage keys is the orignal page web part id's by the original storage keys
            //NewWebPartIDMappings is original web part id's to new web part id's

            //Find the storage key for each new web part
            foreach (var newPartId in newWebPartIdMappings)
            {
                var wpId = newPartId.Value;
                if (originalStorageKeyMappings.ContainsKey(newPartId.Key))
                {
                    var oldKey = originalStorageKeyMappings[newPartId.Key];
                    newWikiContent = newWikiContent.Replace(oldKey, wpId);
                }
            }

            return newWikiContent;
        }
    }
}