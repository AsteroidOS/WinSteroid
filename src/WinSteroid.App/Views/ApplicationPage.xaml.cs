using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class ApplicationPage : Page
    {
        public ApplicationPageViewModel ViewModel
        {
            get { return this.DataContext as ApplicationPageViewModel; }
        }

        public ApplicationPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter is string appId)
            {
                this.ViewModel.Load(appId);
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.Reset();

            base.OnNavigatedFrom(e);
        }
    }
}
