using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel
        {
            get { return this.DataContext as SettingsPageViewModel; }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
