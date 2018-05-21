using Windows.Storage;

namespace WinSteroid.Common.Helpers
{
    public static class SettingsHelper
    {
        public static T GetValue<T>(string key, T defaultValue)
        {
            var result = ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out object value);
            if (result && value is T desiredValue)
            {
                return desiredValue;
            }

            return defaultValue;
        }

        public static void SetValue<T>(string key, T value)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }
    }
}
