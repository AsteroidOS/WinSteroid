using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Converters
{
    public class VibrationLevelToNumberConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VibrationLevel vibrationLevel)
            {
                return (double)vibrationLevel;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double number)
            {
                return (VibrationLevel)number;
            }

            return value;
        }
    }
}
