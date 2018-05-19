using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel
        {
            get { return this.DataContext as MainPageViewModel; }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
