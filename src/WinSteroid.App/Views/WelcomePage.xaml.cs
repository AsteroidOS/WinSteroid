using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePageViewModel ViewModel
        {
            get { return this.DataContext as WelcomePageViewModel; }
        }

        public static WelcomePage Current;

        public WelcomePage()
        {
            Current = this;
            this.InitializeComponent();
        }
    }
}
