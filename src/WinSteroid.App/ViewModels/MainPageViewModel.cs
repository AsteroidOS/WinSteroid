using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
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
            this.BusyMessage = "Initializing...";

            this.BatteryLevelProgressEventHandler = new TypedEventHandler<BackgroundTaskRegistration, BackgroundTaskProgressEventArgs>(OnBatteryProgress);
            await this.InitializeBatteryLevelHandlers();
            await this.InizializeScreenshotContentHandlers();

            this.IsBusy = false;
            this.BusyMessage = string.Empty;

            this.Initialized = true;
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(false);
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

        private async Task InitializeBatteryLevelHandlers()
        {
            this.BackgroundService.RegisterBatteryLevelBackgroundTaskEventHandler(OnBatteryProgress);

            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();

            this.BatteryPercentage = batteryPercentage;
            this.BatteryLevel = BatteryHelper.Parse(batteryPercentage);
        }

        private async Task InizializeScreenshotContentHandlers()
        {
            var result = await this.DeviceService.RegisterToScreenshotContentService();
            if (!result)
            {
                await this.DialogService.ShowMessage("I cannot be able to finish screenshot content handlers registration!", "Error");
            }
        }

        private async void OnBatteryProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var batteryPercentage = Convert.ToInt32(args.Progress);

                this.BatteryPercentage = batteryPercentage;
                this.BatteryLevel = BatteryHelper.Parse(batteryPercentage);
            });
        }
    }
}
