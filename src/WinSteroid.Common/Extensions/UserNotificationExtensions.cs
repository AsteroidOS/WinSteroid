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

            var toastBinding = userNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (toastBinding == null) return string.Empty;

            var textElements = toastBinding.GetTextElements();

            return textElements.FirstOrDefault()?.Text ?? userNotification.AppInfo.DisplayInfo.DisplayName;
        }
    }
}
