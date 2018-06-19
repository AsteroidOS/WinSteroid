//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.ViewManagement;
using WinSteroid.Common;
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

        private TypedEventHandler<UserNotificationListener, UserNotificationChangedEventArgs> NotificationChangedHandler = null;

        public void RegisterNotificationsChangedHandler(TypedEventHandler<UserNotificationListener, UserNotificationChangedEventArgs> notificationChangedHandler)
        {
            if (this.NotificationChangedHandler != null) return;

            this.NotificationChangedHandler = notificationChangedHandler;
            this.UserNotificationListener.NotificationChanged += this.NotificationChangedHandler;
        }

        public void UnregisterNotificationsChangedHandler()
        {
            if (this.NotificationChangedHandler == null) return;

            try
            {
                this.UserNotificationListener.NotificationChanged -= this.NotificationChangedHandler;
                this.NotificationChangedHandler = null;
            }
            catch
            {
                //Code or nothing
            }
        }

        public async Task<bool> RequestAccessAsync()
        {
            var status = UserNotificationListenerAccessStatus.Unspecified;
            if (ApiHelper.IsMobileSystem())
            {
                status = this.UserNotificationListener.GetAccessStatus();
                if (status == UserNotificationListenerAccessStatus.Allowed) return true;
            }

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

            SettingsHelper.SetValue(Constants.LastNotificationIdsSettingKey, value);
        }

        public IReadOnlyList<string> GetLastNotificationIds()
        {
            var value = SettingsHelper.GetValue(Constants.LastNotificationIdsSettingKey, string.Empty);
            if (string.IsNullOrWhiteSpace(value)) return new string[0];

            return value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void ManageNotificationAction(IBuffer buffer)
        {
            var bytes = new byte[buffer.Length];

            DataReader.FromBuffer(buffer).ReadBytes(bytes);

            if (bytes?.Length > 0)
            {
                var @string = Encoding.UTF8.GetString(bytes);
                ToastsHelper.Show("Test action", @string);
            }
        }

        public Task ShowBusySystemTrayAsync(string text)
        {
            if (!ApiHelper.IsSystemTrayAvailable()) return Task.CompletedTask;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = text;
            return statusBar.ProgressIndicator.ShowAsync().AsTask();
        }

        public Task HideBusySystemTrayAsync()
        {
            if (!ApiHelper.IsSystemTrayAvailable()) return Task.CompletedTask;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = string.Empty;
            return statusBar.ProgressIndicator.HideAsync().AsTask();
        }
    }
}
