using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinSteroid.App.Converters
{
    public class NumberToPercentageTextConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt16(value ?? 0) + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
