using Windows.Foundation.Metadata;

namespace WinSteroid.App.Helpers
{
    public static class ApiHelper
    {
        public static bool CheckIfSystemSupportNotificationListener()
        {
            return ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener");
        }
    }
}
