using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Converters
{
    public class NumberToVibrationLevelStringConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double number)
            {
                return ((VibrationLevel)number).ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
