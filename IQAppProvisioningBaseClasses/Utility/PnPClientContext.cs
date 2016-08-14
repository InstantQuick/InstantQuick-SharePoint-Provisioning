﻿using Microsoft.SharePoint.Client;
using System;
using System.Reflection;

// Originally from https://github.com/OfficeDev/PnP-Sites-Core
// This project doesn't take a dependency on the whole of PnPCore
// So instead, it uses the source and gives credit where it is due!
// This stuff is really good.

namespace FromPnPCore
{
    public class PnPClientContext : ClientContext
    {
        public int RetryCount { get; set; }
        public int Delay { get; set; }

        public static PnPClientContext ConvertFrom(ClientContext clientContext, int retryCount = 10, int delay = 500)
        {
            var context = new PnPClientContext(clientContext.Url, retryCount, delay);

            context.AuthenticationMode = clientContext.AuthenticationMode;

            // In case of using networkcredentials in on premises or SharePointOnlineCredentials in Office 365
            if (clientContext.Credentials != null)
            {
                context.Credentials = clientContext.Credentials;
            }
            else
            {
                //Take over the form digest handling setting
                context.FormDigestHandlingEnabled = clientContext.FormDigestHandlingEnabled;

                // In case of app only or SAML
                context.ExecutingWebRequest += delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                {
                    // Call the ExecutingWebRequest delegate method from the original ClientContext object, but pass along the webRequestEventArgs of 
                    // the new delegate method
                    MethodInfo methodInfo = clientContext.GetType().GetMethod("OnExecutingWebRequest", BindingFlags.Instance | BindingFlags.NonPublic);
                    object[] parametersArray = new object[] { webRequestEventArgs };
                    methodInfo.Invoke(clientContext, parametersArray);
                };
            }

            return context;
        }

        /// <summary>
        /// Creates a ClientContext allowing you to override the default retry and delay values of ExecuteQueryRetry
        /// </summary>
        /// <param name="url"></param>
        /// <param name="retryCount"></param>
        /// <param name="delay"></param>
        public PnPClientContext(string url, int retryCount = 10, int delay = 500) : base(url)
        {
            this.RetryCount = retryCount;
            this.Delay = delay;
        }

        /// <summary>
        /// Creates a ClientContext allowing you to override the default retry and delay values of ExecuteQueryRetry
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="retryCount"></param>
        /// <param name="delay"></param>
        public PnPClientContext(Uri uri, int retryCount = 10, int delay = 500) : base(uri)
        {
            this.RetryCount = retryCount;
            this.Delay = delay;
        }

        /// <summary>
        /// Clones a PnPClientContext object while "taking over" the security context of the existing PnPClientContext instance
        /// </summary>
        /// <param name="siteUrl">Site url to be used for cloned ClientContext</param>
        /// <returns>A PnPClientContext object created for the passed site url</returns>
        public PnPClientContext Clone(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                throw new ArgumentException("SiteUrl is required");
            }
            return Clone(new Uri(siteUrl));
        }

        /// <summary>
        /// Clones a PnPClientContext object while "taking over" the security context of the existing PnPClientContext instance
        /// </summary>
        /// <param name="siteUri">Site url to be used for cloned ClientContext</param>
        /// <returns>A PnPClientContext object created for the passed site url</returns>
        public PnPClientContext Clone(Uri siteUri)
        {
            if (siteUri == null)
            {
                throw new ArgumentException("SiteUrl is required");
            }

            PnPClientContext clonedClientContext = new PnPClientContext(siteUri);
            clonedClientContext.RetryCount = this.RetryCount;
            clonedClientContext.Delay = this.Delay;

            clonedClientContext.AuthenticationMode = this.AuthenticationMode;

            // In case of using networkcredentials in on premises or SharePointOnlineCredentials in Office 365
            if (this.Credentials != null)
            {
                clonedClientContext.Credentials = this.Credentials;
            }
            else
            {
                //Take over the form digest handling setting
                clonedClientContext.FormDigestHandlingEnabled = (this as ClientContext).FormDigestHandlingEnabled;

                // In case of app only or SAML
                clonedClientContext.ExecutingWebRequest += delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                {
                    // Call the ExecutingWebRequest delegate method from the original ClientContext object, but pass along the webRequestEventArgs of 
                    // the new delegate method
                    MethodInfo methodInfo = this.GetType().GetMethod("OnExecutingWebRequest", BindingFlags.Instance | BindingFlags.NonPublic);
                    object[] parametersArray = new object[] { webRequestEventArgs };
                    methodInfo.Invoke(this, parametersArray);
                };
            }

            return clonedClientContext;
        }

    }
}
