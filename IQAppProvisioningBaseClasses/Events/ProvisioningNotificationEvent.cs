using System;

namespace IQAppProvisioningBaseClasses.Events
{
    public delegate void ProvisioningNotificationEventHandler(object sender, ProvisioningNotificationEventArgs eventArgs);

    public enum ProvisioningNotificationLevels
    {
        Normal,
        Verbose
    }

    public class ProvisioningNotificationEventArgs : EventArgs
    {
        public ProvisioningNotificationLevels Level { get; set; }
        public string Detail { get; set; }
    }
}
