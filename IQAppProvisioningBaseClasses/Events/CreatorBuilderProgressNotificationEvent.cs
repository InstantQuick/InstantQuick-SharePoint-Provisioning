using System;

namespace IQAppProvisioningBaseClasses.Events
{
    public delegate void CreatorBuilderProgressNotificationEventHandler(
        object sender, CreatorBuilderProgressNotificationEvent eventArgs);

    public class CreatorBuilderProgressNotificationEvent : EventArgs
    {
        public string Message { get; set; }
    }
}