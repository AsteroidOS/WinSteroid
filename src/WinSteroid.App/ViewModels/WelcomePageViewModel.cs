using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class WelcomePageViewModel : BasePageViewModel
    {
        private readonly DeviceService DeviceService;

        public WelcomePageViewModel(
            DeviceService deviceService,
            IDialogService dialogService,
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            this.Initialize();
        }

        public override void Initialize()
        {
            var deviceId = this.DeviceService.GetLastSavedDeviceId();
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                this.DeviceId = deviceId;
                this.Pair();
            }

            this.Initialized = true;
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        private string _deviceId;
        public string DeviceId
        {
            get { return _deviceId; }
            set { Set(nameof(DeviceId), ref _deviceId, value); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { Set(nameof(DeviceName), ref _deviceName, value); }
        }

        private RelayCommand _startSearchCommand;
        public RelayCommand StartSearchCommand
        {
            get
            {
                if (_startSearchCommand == null)
                {
                    _startSearchCommand = new RelayCommand(StartSearch);
                }

                return _startSearchCommand;
            }
        }

        private async void StartSearch()
        {
            if (!string.IsNullOrWhiteSpace(this.DeviceId))
            {
                this.Pair();
                return;
            }

            var element = (FrameworkElement)Views.WelcomePage.Current;

            var result = await this.DeviceService.PickSingleDeviceAsync(element);
            if (!result) return;

            this.DeviceId = this.DeviceService.Current.Id;
            this.DeviceName = this.DeviceService.Current.Name;

            this.Pair();
        }
        
        private RelayCommand _pairCommand;
        public RelayCommand PairCommand
        {
            get
            {
                if (_pairCommand == null)
                {
                    _pairCommand = new RelayCommand(Pair, CanPair);
                }

                return _pairCommand;
            }
        }

        public bool CanPair()
        {
            return !string.IsNullOrWhiteSpace(this.DeviceId) || this.DeviceService.Current != null;
        }

        private async void Pair()
        {
            this.IsBusy = true;
            this.BusyMessage = "Pairing";

            if (string.IsNullOrWhiteSpace(this.DeviceId) && this.DeviceService.Current != null)
            {
                this.DeviceId = this.DeviceService.Current.Id;
                this.DeviceName = this.DeviceService.Current.Name;
            }

            var connected = await this.DeviceService.ConnectAsync(this.DeviceId);
            if (!connected)
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                await this.DialogService.ShowMessage("I cannot connect to selected Bluetooth device", "Error");
                return;
            }

            var pairingResult = await this.DeviceService.PairAsync();
            if (!pairingResult.IsSuccess)
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                await this.DialogService.ShowMessage("Paired operation failed", "Error");
                return;
            }

            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Main));
        }
    }
}
