using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace WinSteroid.Common.Helpers
{
    public static class ToastsHelper
    {
        public static void Send(string title, string message)
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
