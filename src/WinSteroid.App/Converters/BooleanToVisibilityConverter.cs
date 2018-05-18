using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinSteroid.App.Converters
{
    public class BooleanToVisibilityConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                return boolean ? Visibility.Visible : Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
