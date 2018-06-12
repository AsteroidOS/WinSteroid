using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace WinSteroid.Common.Models
{
    public static class BatteryLevelExtensions
    {
        public static Color GetColor(this BatteryLevel batteryLevel)
        {
            switch (batteryLevel)
            {
                case BatteryLevel.Good:
                case BatteryLevel.Discrete:
                    return Colors.Green;
                case BatteryLevel.Bad:
                    return Colors.Orange;
                case BatteryLevel.Critic:
                    return Colors.Red;
                case BatteryLevel.Dead:
                default:
                    return (Application.Current.Resources["SystemControlBackgroundChromeMediumBrush"] as SolidColorBrush).Color;
            }
        }
    }
}
