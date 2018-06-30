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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.ViewManagement;
using WinSteroid.Common.Helpers;

namespace WinSteroid.Common.Notifications
{
    public static class NotificationsManager
    {
        private static readonly UserNotificationListener UserNotificationListener = UserNotificationListener.Current;

        public static async Task<bool> RequestAccessAsync()
        {
            var status = UserNotificationListenerAccessStatus.Unspecified;
            if (ApiHelper.IsMobileSystem())
            {
                status = UserNotificationListener.GetAccessStatus();
                if (status == UserNotificationListenerAccessStatus.Allowed) return true;
            }

            status = await UserNotificationListener.RequestAccessAsync();
            return status == UserNotificationListenerAccessStatus.Allowed;
        }

        public static IAsyncOperation<IReadOnlyList<UserNotification>> RetriveNotificationsAsync()
        {
            return UserNotificationListener.GetNotificationsAsync(NotificationKinds.Toast);
        }

        public static void ManageNotificationAction(IBuffer buffer)
        {
            //var bytes = new byte[buffer.Length];

            //DataReader.FromBuffer(buffer).ReadBytes(bytes);

            //if (bytes?.Length > 0)
            //{
            //    var @string = Encoding.UTF8.GetString(bytes);
            //}
        }

        public static Task ShowBusySystemTrayAsync(string text)
        {
            if (!ApiHelper.IsSystemTrayAvailable()) return Task.CompletedTask;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = text;
            return statusBar.ProgressIndicator.ShowAsync().AsTask();
        }

        public static Task HideBusySystemTrayAsync()
        {
            if (!ApiHelper.IsSystemTrayAvailable()) return Task.CompletedTask;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = string.Empty;
            return statusBar.ProgressIndicator.HideAsync().AsTask();
        }
    }
}
