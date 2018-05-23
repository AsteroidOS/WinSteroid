using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            this.Unloaded += OnPageUnloaded;
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Clear<ApplicationPageViewModel>();
        }
    }
}
