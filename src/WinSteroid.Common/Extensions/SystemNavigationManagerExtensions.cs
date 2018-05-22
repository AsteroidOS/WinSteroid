using System;
using Windows.UI.Xaml.Controls;

namespace Windows.UI.Core
{
    public static class SystemNavigationManagerExtensions
    {
        public static void UpdateAppViewBackButtonVisibility(this SystemNavigationManager systemNavigationManager, Frame frame)
        {
            if (systemNavigationManager == null)
            {
                throw new ArgumentNullException(nameof(systemNavigationManager));
            }

            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            systemNavigationManager.AppViewBackButtonVisibility = frame.CanGoBack 
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
        }
    }
}
