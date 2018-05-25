using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinSteroid.App.Converters
{
    public class BooleanToCornerRadiusConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                return new CornerRadius(boolean ? 120 : 24);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
