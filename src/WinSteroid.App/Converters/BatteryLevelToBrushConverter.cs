using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Converters
{
    class BatteryLevelToBrushConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BatteryLevel batteryLevel)
            {
                switch (batteryLevel)
                {
                    case BatteryLevel.Good:
                    case BatteryLevel.Discrete:
                        return new SolidColorBrush(Colors.Green);
                    case BatteryLevel.Bad:
                        return new SolidColorBrush(Colors.Orange);
                    case BatteryLevel.Critic:
                        return new SolidColorBrush(Colors.Red);
                    case BatteryLevel.Dead:
                        return new SolidColorBrush(Colors.Gray);
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
