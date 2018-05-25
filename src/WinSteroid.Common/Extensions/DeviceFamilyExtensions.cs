using Windows.System.Profile;

namespace WinSteroid.Common.Models
{
    public static class DeviceFamilyExtensions
    {
        public static DeviceFamily Parse(string @string)
        {
            switch (@string)
            {
                case "Windows.Desktop":
                    return DeviceFamily.Desktop;
                case "Windows.Mobile":
                    return DeviceFamily.Mobile;
                case "Windows.Xbox":
                    return DeviceFamily.Xbox;
                case "Windows.IoT":
                    return DeviceFamily.IoT;
                case "Windows.Holographic":
                    return DeviceFamily.Holographic;
                case "Windows.Team":
                    return DeviceFamily.Team;
                default:
                    return DeviceFamily.Unknown;
            }
        }
        
        public static DeviceFamily CurrentDeviceFamily
        {
            get { return Parse(AnalyticsInfo.VersionInfo.DeviceFamily); }
        }
    }
}
