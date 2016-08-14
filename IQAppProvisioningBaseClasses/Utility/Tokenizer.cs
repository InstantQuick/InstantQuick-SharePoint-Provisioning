using Microsoft.SharePoint.Client;

namespace IQAppProvisioningBaseClasses.Utility
{
    public static class Tokenizer
    {
        public static string TokenizeUrls(Web web, string text)
        {
            text = text.Replace(web.Url, "{@WebUrl}");
            if (web.ServerRelativeUrl != "/") text = text.Replace(web.ServerRelativeUrl, "{@WebServerRelativeUrl}");
            return text;
        }

        public static string ReplaceUrlTokens(Web web, string text)
        {
            text = text ?? string.Empty;
            return text.Replace("{@WebUrl}", web.Url).Replace("{@WebServerRelativeUrl}", web.ServerRelativeUrl);
        }
    }
}
