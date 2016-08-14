using System;
using IQAppProvisioningBaseClasses.Events;

namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ProvisioningManagerBase
    {
        public event ProvisioningNotificationEventHandler Notify;
        public bool WriteNotificationsToStdOut { get; set; }

        protected void OnNotify(ProvisioningNotificationLevels level, string detail)
        {
            if (Notify != null)
            {
                Notify(null, new ProvisioningNotificationEventArgs
                {
                    Level = level,
                    Detail = detail
                });
            }
            else if (WriteNotificationsToStdOut)
            {
                Console.Out.WriteLine(detail);
            }
        }
    }
}