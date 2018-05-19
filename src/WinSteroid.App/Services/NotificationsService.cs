using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace WinSteroid.App.Services
{
    public class NotificationsService
    {
        private readonly UserNotificationListener UserNotificationListener;

        public NotificationsService()
        {
            this.UserNotificationListener = UserNotificationListener.Current;
        }

        public async Task InitializeAsync()
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

        public Task<IReadOnlyList<UserNotification>> RetriveNotificationsAsync()
        {
            return UserNotificationListener.GetNotificationsAsync(NotificationKinds.Toast).AsTask();
        }
    }
}
