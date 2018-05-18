using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace WinSteroid.App.Services
{
    public class NotificationsService
    {
        private static UserNotificationListener UserNotificationListener
        {
            get { return UserNotificationListener.Current; }
        }

        public static async Task InitializeAsync()
        {
            var status = await UserNotificationListener.RequestAccessAsync();
            switch (status)
            {
                case UserNotificationListenerAccessStatus.Allowed:
                    //Show success toast
                    return;
                case UserNotificationListenerAccessStatus.Denied:
                    // :( show message
                    return;
                case UserNotificationListenerAccessStatus.Unspecified:
                    //WTF show info message
                    return;
            }
        }

        public static async Task<IEnumerable<UserNotification>> RetriveNotificationsAsync()
        {
            var userNotifications = await UserNotificationListener.GetNotificationsAsync(NotificationKinds.Toast);

            return userNotifications.ToArray();
        }
    }
}
