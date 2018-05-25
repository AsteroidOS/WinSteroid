using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace WinSteroid.App.Converters
{
    public class BooleanToBrushConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                var solidColorBrushResourceKey = boolean ? "BackButtonDisabledForegroundThemeBrush" : "SystemControlBackgroundAccentBrush";
                return Application.Current.Resources[solidColorBrushResourceKey] as SolidColorBrush;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
