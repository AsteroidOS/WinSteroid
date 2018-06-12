using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinSteroid.App.Converters
{
    public class BooleanToGridLengthConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                if (parameter is char star) return new GridLength(1.0, GridUnitType.Star);

                if (parameter is string s && s.Contains("*"))
                {
                    var result = double.TryParse(s, out double starsNumber);
                    if (result) return new GridLength(starsNumber, GridUnitType.Star);
                }

                return boolean ? new GridLength(1.0, GridUnitType.Star) : GridLength.Auto;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
