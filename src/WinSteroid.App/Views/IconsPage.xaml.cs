using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class IconsPage : Page
    {
        public IconsPageViewModel ViewModel
        {
            get { return this.DataContext as IconsPageViewModel; }
        }

        public IconsPage()
        {
            this.InitializeComponent();
        }
    }
}
