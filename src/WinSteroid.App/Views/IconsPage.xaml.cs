using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel.RefreshIconsPreferences();

            base.OnNavigatedTo(e);
        }
    }
}
