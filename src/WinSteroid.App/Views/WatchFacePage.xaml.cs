using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class WatchFacePage : Page
    {
        public WatchFacePageViewModel ViewModel
        {
            get { return this.DataContext as WatchFacePageViewModel; }
        }

        public WatchFacePage()
        {
            this.InitializeComponent();
        }
    }
}
