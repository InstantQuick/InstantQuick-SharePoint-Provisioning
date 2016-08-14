using System;
using System.Collections.Generic;
using IQAppProvisioningBaseClasses.Provisioning;

namespace IQAppProvisioningBaseClasses
{
    public class WebPartPageUtility
    {
        public static List<WebPartZoneMapping> GetWebPartZoneMappings(string page, Dictionary<string, string> webParts)
        {
            var zoneMappings = new List<WebPartZoneMapping>();

            var zoneContents = GetZoneContents(page);

            foreach (var zoneContent in zoneContents)
            {
                foreach (var webPartId in webParts.Keys)
                {
                    if (zoneContent.Value.Contains(webPartId))
                    {
                        //The position of the ID in the zone is used to sort at provisioning time
                        //so that the web parts are in the correct order on the page
                        zoneMappings.Add(new WebPartZoneMapping
                        {
                            ZoneId = zoneContent.Key,
                            Position = zoneContent.Value.IndexOf(webPartId, StringComparison.Ordinal),
                            WebPartId = webPartId
                        });
                    }
                }
            }
            return zoneMappings;
        }

        private static Dictionary<string, string> GetZoneContents(string page)
        {
            var zoneContents = new Dictionary<string, string>();
            var zones = page.GetInnerTextList("<WebPartPages:WebPartZone", "</WebPartPages:WebPartZone>");
            foreach (var zone in zones)
            {
                var id = zone.GetInnerText("ID=\"", "\"", true);
                if (id != string.Empty)
                {
                    zoneContents.Add(id, zone);
                }
            }
            return zoneContents;
        }
    }
}