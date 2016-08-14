using System;
using IQAppProvisioningBaseClasses.Events;

namespace IQAppManifestBuilders
{
    public class CreatorBuilderBase
    {
        /// <summary>
        /// Notification event
        /// </summary>
        public event CreatorBuilderProgressNotificationEventHandler VerboseNotify;

        /// <summary>
        /// If true causes the top of the event chain to write the notification via Console.Out
        /// instead of propogating the event.
        /// 
        /// If you use event propogation from c# in a PowerShell module, none of the notifications display until
        /// the entire operation finishes.
        /// </summary>
        public bool WriteNotificationsToStdOut { get; set; }

        /// <summary>
        /// Raises an event with an informative message or writes it to stdout
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnVerboseNotify(string message)
        {
            if (VerboseNotify != null)
            {
                VerboseNotify(null, new CreatorBuilderProgressNotificationEvent
                {
                    Message = message
                });
            }
            else if (WriteNotificationsToStdOut)
            {
                Console.Out.WriteLine(message);
            }
        }

        /// <summary>
        /// Not currently used
        /// </summary>
        public event CreatorBuilderProgressNotificationEventHandler InformationNotify;

        /// <summary>
        /// Not currently used
        /// </summary>
        /// <param name="message"></param>
        protected void OnInformationNotify(string message)
        {
            if (InformationNotify != null)
            {
                InformationNotify(null, new CreatorBuilderProgressNotificationEvent
                {
                    Message = message
                });
            }
            else if (WriteNotificationsToStdOut)
            {
                Console.Out.WriteLine(message);
            }
        }
    }
}