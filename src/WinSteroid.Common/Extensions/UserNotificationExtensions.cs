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
using System.Linq;

namespace Windows.UI.Notifications
{
    public static class UserNotificationExtensions
    {
        public static string GetBody(this UserNotification userNotification)
        {
            if (userNotification == null)
            {
                throw new ArgumentNullException(nameof(userNotification));
            }

            var toastBinding = userNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (toastBinding == null) return string.Empty;

            var textElements = toastBinding.GetTextElements();
            
            return string.Join("\n", textElements.Skip(1).Select(t => t.Text));
        }

        public static string GetTitle(this UserNotification userNotification)
        {
            if (userNotification == null)
            {
                throw new ArgumentNullException(nameof(userNotification));
            }

            try
            {
                var toastBinding = userNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
                if (toastBinding == null) return string.Empty;

                var textElements = toastBinding.GetTextElements();

                return textElements.FirstOrDefault()?.Text ?? userNotification.AppInfo.DisplayInfo.DisplayName;
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return userNotification.AppInfo.DisplayInfo.DisplayName;
            }
        }

        public static Uri GetLaunchUri(this UserNotification userNotification)
        {
            throw new NotImplementedException();
        }
    }
}
