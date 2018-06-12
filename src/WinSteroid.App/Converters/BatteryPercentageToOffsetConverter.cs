using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinSteroid.App.Converters
{
    public class BatteryPercentageToOffsetConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int percentage)
            {
                double offsetValue = 100 - percentage;
                if (offsetValue < 0)
                {
                    offsetValue = 0;
                }

                return Math.Round(offsetValue / 100, 2);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
