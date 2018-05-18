using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using WinSteroid.App.Data;

namespace WinSteroid.App.Services
{
    public class NotificationsService
    {
        private readonly Database Database;
        private readonly UserNotificationListener UserNotificationListener;

        public NotificationsService(Database database)
        {
            this.Database = database ?? throw new ArgumentNullException(nameof(database));
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

        public async Task<IEnumerable<UserNotification>> RetriveNotificationsAsync()
        {
            var userNotifications = await UserNotificationListener.GetNotificationsAsync(NotificationKinds.Toast);

            var newNotifications = new List<UserNotification>();

            foreach (var userNotification in userNotifications)
            {
                var existsNotification = await this.Database.ExistsNotificationWithId(userNotification.Id.ToString());
                if (existsNotification) continue;

                await this.Database.InsertNotificationAsync(userNotification);
                newNotifications.Add(userNotification);
            }

            return newNotifications;
        }
    }
}
