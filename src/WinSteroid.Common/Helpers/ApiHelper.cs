using Windows.Foundation.Metadata;

namespace WinSteroid.Common.Helpers
{
    public static class ApiHelper
    {
        public static bool CheckIfSystemSupportNotificationListener()
        {
            return ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener");
        }

        public static bool CheckIfSystemIsMobile()
        {
            return ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
        }
    }
}
