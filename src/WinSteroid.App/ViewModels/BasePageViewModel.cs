using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;

namespace WinSteroid.App.ViewModels
{
    public abstract class BasePageViewModel : ViewModelBase
    {
        public readonly IDialogService DialogService;
        public readonly INavigationService NavigationService;

        public BasePageViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        public abstract void Initialize();

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(nameof(IsBusy), ref _isBusy, value); }
        }

        private string _busyMessage;
        public string BusyMessage
        {
            get { return _busyMessage; }
            set { Set(nameof(BusyMessage), ref _busyMessage, value); }
        }

        public bool Initialized { get; internal set; }

        private RelayCommand _backCommand;
        public RelayCommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new RelayCommand(GoBack);
                }

                return _backCommand;
            }
        }

        public abstract Task<bool> CanGoBack();

        private async void GoBack()
        {
            var canGoBack = await CanGoBack();
            if (!canGoBack) return;

            this.NavigationService.GoBack();
        }
    }
}
