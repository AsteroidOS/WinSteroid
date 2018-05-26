using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class ApplicationsPage : Page
    {
        public ApplicationsPageViewModel ViewModel
        {
            get { return this.DataContext as ApplicationsPageViewModel; }
        }

        public ApplicationsPage()
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
