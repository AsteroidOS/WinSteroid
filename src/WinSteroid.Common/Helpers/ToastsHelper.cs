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

using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel;
using Windows.UI.Notifications;

namespace WinSteroid.Common.Helpers
{
    public static class ToastsHelper
    {
        public static void Show(string message, params string[] actionArgs) => Show(Package.Current.DisplayName, message);

        public static void Show(string title, string message, params string[] actionArgs)
        {
            var toastVisual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText { Text = title },
                        new AdaptiveText { Text = message }
                    }
                }
            };

            var toastContent = new ToastContent
            {

                Visual = toastVisual
            };

            var toastNotification = new ToastNotification(toastContent.GetXml());

            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }
    }
}
