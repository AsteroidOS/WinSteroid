using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;

namespace WinSteroid.App.ViewModels.Tutorials
{
    public class UsbPageViewModel : BasePageViewModel
    {
        public UsbPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {

        }

        public override void Reset()
        {

        }
    }
}
