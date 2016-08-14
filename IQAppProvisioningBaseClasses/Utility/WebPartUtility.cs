using System;
using System.Net;
using IQAppProvisioningBaseClasses.WebPartPagesWebService;
using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses
{
    public class WebPartUtility
    {
        public static string GetWebPartPage(ClientContext ctx, Web web, string serverRelativeFileUrl)
        {
            var webPartPageService = new WebPartPagesWebService.WebPartPagesWebService
            {
                Url = web.Url + "/_vti_bin/webpartpages.asmx"
            };

            //TODO: Network credentials
            var credentials = ctx.Credentials as SharePointOnlineCredentials;
            if (credentials != null)
            {
                var authCookieString = credentials.GetAuthenticationCookie(new Uri(web.Url));
                string[] parts =
                {
                    authCookieString.Substring(0, authCookieString.IndexOf('=')),
                    authCookieString.Substring(authCookieString.IndexOf('=') + 1)
                };
                webPartPageService.CookieContainer = new CookieContainer();
                var cookie = new Cookie(parts[0], parts[1]) {Domain = new Uri(web.Url).Host};
                webPartPageService.CookieContainer.Add(cookie);
            }
            else
            {
                //Assumes the site is local intranet and that saved credentials exist in the credential manager 
                webPartPageService.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            var page = webPartPageService.GetWebPartPage(serverRelativeFileUrl, SPWebServiceBehavior.Version3);
            if (page != null && page.IndexOf("<%@ ", StringComparison.Ordinal) != -1)
            {
                page = page.Substring(page.IndexOf("<%@ ", StringComparison.Ordinal));
            }

            return page;
        }

        public static string GetWebPart(ClientContext ctx, Web web, string serverRelativeFileUrl, Guid id)
        {
            var webPartPageService = new WebPartPagesWebService.WebPartPagesWebService
            {
                Url = web.Url + "/_vti_bin/webpartpages.asmx"
            };

            //TODO: Network credentials
            var credentials = ctx.Credentials as SharePointOnlineCredentials;
            if (credentials != null)
            {
                var authCookieString = credentials.GetAuthenticationCookie(new Uri(web.Url));
                string[] parts =
                {
                    authCookieString.Substring(0, authCookieString.IndexOf('=')),
                    authCookieString.Substring(authCookieString.IndexOf('=') + 1)
                };
                webPartPageService.CookieContainer = new CookieContainer();
                var cookie = new Cookie(parts[0], parts[1]) {Domain = new Uri(web.Url).Host};
                webPartPageService.CookieContainer.Add(cookie);
            }
            else
            {
                //Assumes the site is local intranet and that saved credentials exist in the credential manager 
                webPartPageService.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            var part = webPartPageService.GetWebPart2(serverRelativeFileUrl, id, Storage.Shared,
                SPWebServiceBehavior.Version3);

            return part;
        }

        public static string GetWebPartPage(Web web, NetworkCredential credentials, string serverRelativeFileUrl)
        {
            var webPartPageService = new WebPartPagesWebService.WebPartPagesWebService
            {
                Url = web.Url + "/_vti_bin/webpartpages.asmx",
                Credentials = credentials
            };


            var page = webPartPageService.GetWebPartPage(serverRelativeFileUrl, SPWebServiceBehavior.Version3);
            if (page != null && page.IndexOf("<%@ ", StringComparison.Ordinal) != -1)
            {
                if (page.Length > page.IndexOf("<%@ ", StringComparison.Ordinal))
                    page = page.Substring(page.IndexOf("<%@ ", StringComparison.Ordinal));
            }

            return page;
        }

        public static string GetWebPart(Web web, NetworkCredential credentials, string serverRelativeFileUrl, Guid id)
        {
            var webPartPageService = new WebPartPagesWebService.WebPartPagesWebService
            {
                Url = web.Url + "/_vti_bin/webpartpages.asmx",
                Credentials = credentials
            };

            //Assumes the site is local intranet and that saved credentials exist in the credential manager 

            var part = webPartPageService.GetWebPart2(serverRelativeFileUrl, id, Storage.Shared,
                SPWebServiceBehavior.Version3);

            return part;
        }

        public static string LookForWebPartId(string page, string storageKey)
        {
            //Look for it in an element where the ID comes before the web part id
            var foundText =
                page.GetInnerText(" ID=\"" + StorageKeyFromWikiContentId(storageKey) + "\"", ">", true)
                    .GetInnerText("WebPartId=\"{", "}\"", true);

            if (foundText == string.Empty)
            {
                var storageKeyIndex = page.IndexOf(StorageKeyFromWikiContentId(storageKey),
                    StringComparison.InvariantCultureIgnoreCase);
                if (storageKeyIndex == -1) return string.Empty;
                var elementStartIndex = page.Substring(0, storageKeyIndex)
                    .LastIndexOf("WebPartId", StringComparison.Ordinal);
                if (elementStartIndex != -1)
                {
                    //Look before the storage key
                    foundText = page.Substring(elementStartIndex).GetInnerText("WebPartId=\"{", "}\"", true);
                }
                else
                {
                    //Look after the storage key
                    elementStartIndex = page.Substring(storageKeyIndex).IndexOf("WebPartId", StringComparison.Ordinal) +
                                        storageKeyIndex;
                    if (elementStartIndex != -1)
                        foundText = page.Substring(elementStartIndex).GetInnerText("WebPartId=\"{", "}\"", true);
                }
            }
            return foundText.ToLower();
        }

        public static string StorageKeyFromWikiContentId(string id)
        {
            var g = id.Replace("{", "").Replace("}", "").Split('-');
            return $"g_{g[0]}_{g[1]}_{g[2]}_{g[3]}_{g[4]}";
        }

        public static string StorageKeyToWikiContentId(string id)
        {
            var g = id.Split('_');
            return $"{g[1]}-{g[2]}-{g[3]}-{g[4]}-{g[5]}";
        }
    }
}