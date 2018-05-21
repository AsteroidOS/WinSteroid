using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Converters
{
    public class IoniconsConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ApplicationIcon icon)
            {
                switch (icon)
                {
                    case ApplicationIcon.Alert:
                        return "&#xf104;";
                    case ApplicationIcon.Apps:
                        return "&#xf10a;";
                    case ApplicationIcon.Calendar:
                        return "&#xf3f4;";
                    case ApplicationIcon.Call:
                        return "&#xf13e;";
                    case ApplicationIcon.Message:
                        return "&#xf1b8;";
                    case ApplicationIcon.Photo:
                        return "&#xf3f6;";
                    case ApplicationIcon.Settings:
                        return "&#xf4a7;";
                    case ApplicationIcon.SMS:
                        return "&#xf20c;";
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
