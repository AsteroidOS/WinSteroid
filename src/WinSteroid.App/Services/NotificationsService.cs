﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.Services
{
    public class NotificationsService
    {
        private readonly UserNotificationListener UserNotificationListener;

        public NotificationsService()
        {
            this.UserNotificationListener = UserNotificationListener.Current;
        }

        public async Task<bool> RequestAccessAsync()
        {
            var status = this.UserNotificationListener.GetAccessStatus();
            if (status == UserNotificationListenerAccessStatus.Allowed) return true;

            status = await UserNotificationListener.RequestAccessAsync();
            return status == UserNotificationListenerAccessStatus.Allowed;
        }

        public IAsyncOperation<IReadOnlyList<UserNotification>> RetriveNotificationsAsync()
        {
            return UserNotificationListener.GetNotificationsAsync(NotificationKinds.Toast);
        }

        public void SaveLastNotificationIds(IEnumerable<UserNotification> notifications)
        {
            if (notifications == null)
            {
                notifications = new UserNotification[0];
            }

            this.SaveLastNotificationIds(notifications.Select(notification => notification.Id.ToString()));
        }

        public void SaveLastNotificationIds(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                ids = new string[0];
            }

            var value = ids.Count() > 0 ? string.Join(";", ids) : string.Empty;

            SettingsHelper.SetValue("lastNotificationIds", value);
        }

        public IReadOnlyList<string> GetLastNotificationIds()
        {
            var value = SettingsHelper.GetValue("lastNotificationIds", string.Empty);
            if (string.IsNullOrWhiteSpace(value)) return new string[0];

            return value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
