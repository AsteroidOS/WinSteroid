using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Foundation;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class MainPageViewModel : BasePageViewModel
    {
        private readonly BackgroundService BackgroundService;
        private readonly DeviceService DeviceService;

        private TypedEventHandler<BackgroundTaskRegistration, BackgroundTaskProgressEventArgs> BatteryLevelProgressEventHandler = null;

        public MainPageViewModel(
            BackgroundService backgroundService,
            DeviceService deviceService,
            INavigationService navigationService,
            IDialogService dialogService) : base(dialogService, navigationService)
        {
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            this.Initialize();
        }

        public override async void Initialize()
        {
            this.IsBusy = true;
            this.BusyMessage = "Initializing";

            this.DeviceName = this.DeviceService.Current.Name;
            this.DeviceService.AttachConnectionStatusChangedHandler(OnConnectionStatusChanged);
            this.BatteryLevelProgressEventHandler = OnBatteryProgress;
            this.RegisterBatteryLevelHandler();

            var newPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            var oldPercentage = this.BatteryPercentage;

            this.BatteryPercentage = newPercentage;
            this.BatteryLevel = BatteryHelper.Parse(newPercentage);

            this.IsBusy = false;
            this.BusyMessage = string.Empty;

            Views.MainPage.Current.UpdatePercentage(oldPercentage, newPercentage);

            //await this.InizializeScreenshotContentHandlersAsync();

            App.RemoveWelcomePageFromBackStack();

            this.Initialized = true;
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { Set(nameof(DeviceName), ref _deviceName, value); }
        }

        private bool _isDeviceConnected;
        public bool IsDeviceConnected
        {
            get { return _isDeviceConnected; }
            set { Set(nameof(IsDeviceConnected), ref _isDeviceConnected, value); }
        }

        private int _batteryPercentage;
        public int BatteryPercentage
        {
            get { return _batteryPercentage; }
            set { Set(nameof(BatteryPercentage), ref _batteryPercentage, value); }
        }

        private BatteryLevel _batteryLevel;
        public BatteryLevel BatteryLevel
        {
            get { return _batteryLevel; }
            set { Set(nameof(BatteryLevel), ref _batteryLevel, value); }
        }

        private RelayCommand _takeScreenshotCommand;
        public RelayCommand TakeScreenshotCommand
        {
            get
            {
                if (_takeScreenshotCommand == null)
                {
                    _takeScreenshotCommand = new RelayCommand(this.TakeScreenshot);
                }

                return _takeScreenshotCommand;
            }
        }

        private async void TakeScreenshot()
        {
            await this.DeviceService.TakeScreenshotAsync();
        }

        private RelayCommand _settingsCommand;
        public RelayCommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand(GoToSettings);
                }

                return _settingsCommand;
            }
        }

        private void GoToSettings()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Settings));
        }

        public void RegisterBatteryLevelHandler()
        {
            this.BackgroundService.RegisterBatteryLevelBackgroundTaskEventHandler(OnBatteryProgress);
        }

        public void UnregisterBatteryLevelHandler()
        {
            this.BackgroundService.UnregisterBatteryLevelBackgroundTaskEventHandler(OnBatteryProgress);
        }

        private async Task InizializeScreenshotContentHandlersAsync()
        {
            var result = await this.DeviceService.RegisterToScreenshotContentService();
            if (!result)
            {
                await this.DialogService.ShowMessage("I cannot be able to finish screenshot content handlers registration!", "Error");
            }
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            this.IsDeviceConnected = this.DeviceService.BluetoothDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
        }

        private async void OnBatteryProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var newPercentage = Convert.ToInt32(args.Progress);
                var oldPercentage = this.BatteryPercentage;

                this.BatteryPercentage = newPercentage;
                this.BatteryLevel = BatteryHelper.Parse(newPercentage);

                Views.MainPage.Current.UpdatePercentage(oldPercentage, newPercentage);
            });
        }

        public void Reset()
        {
            this.BatteryLevel = BatteryLevel.Dead;
            this.BatteryPercentage = 0;
        }
    }
}
