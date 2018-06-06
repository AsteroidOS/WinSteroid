using System;
using Windows.UI.Xaml.Media.Imaging;

namespace WinSteroid.App.ViewModels
{
    public class NotificationItemViewModel
    {
        public string Id { get; set; }

        public string AppId { get; set; }

        public string PackageName { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string Icon { get; set; }

        public Uri LaunchUri { get; set; }

        public BitmapImage PackageIcon { get; set; }
    }
}
