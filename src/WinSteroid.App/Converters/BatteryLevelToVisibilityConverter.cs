using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Converters
{
    public class BatteryLevelToVisibilityConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BatteryLevel batteryLevel && parameter is int index)
            {
                switch (batteryLevel)
                {
                    case BatteryLevel.Good:
                    case BatteryLevel.Discrete:
                        return Visibility.Visible;
                    case BatteryLevel.Bad:
                        return index < 3 ? Visibility.Visible : Visibility.Collapsed;
                    case BatteryLevel.Critic:
                        return index < 2 ? Visibility.Visible : Visibility.Collapsed;
                    case BatteryLevel.Dead:
                        return index < 1 ? Visibility.Visible : Visibility.Collapsed;
                    default:
                        return Visibility.Collapsed;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
