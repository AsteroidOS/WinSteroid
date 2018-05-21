using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.App.Helpers;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly NotificationsService NotificationsService;
        private readonly INavigationService NavigationService;
        private readonly IDialogService DialogService;

        public MainPageViewModel(
            DeviceService deviceService, 
            BackgroundService backgroundService, 
            NotificationsService notificationsService, 
            INavigationService navigationService,
            IDialogService dialogService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.NotificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            this.InitializeBatteryLevelHandlers();
            this.InitializeUserNotificationsHandlers();
            //this.InitializeActiveNotificationHandlers();
            this.InizializeScreenshotContentHandlers();
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

        private RelayCommand _updateBatteryStatusCommand;
        public RelayCommand UpdateBatteryStatusCommand
        {
            get
            {
                if (_updateBatteryStatusCommand == null)
                {
                    _updateBatteryStatusCommand = new RelayCommand(UpdateBatteryStatus);
                }

                return _updateBatteryStatusCommand;
            }
        }

        private RelayCommand _takeScreenshotCommand;
        public RelayCommand TakeScreenshotCommand
        {
            get
            {
                if (_takeScreenshotCommand == null)
                {
                    _takeScreenshotCommand = new RelayCommand(this.DeviceService.TakeScreenshotAsync);
                }

                return _takeScreenshotCommand;
            }
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

        private async void InitializeBatteryLevelHandlers()
        {
            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);

            await this.DeviceService.RegisterBatteryPercentageNotification(OnBatteryLevelCharacteristicValueChanged);

            await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            await this.BackgroundService.RegisterBatteryLevelTask(characteristic);

            this.UpdateBatteryStatus();
        }

        //private async void InitializeActiveNotificationHandlers()
        //{
        //    var characteristic = await this.DeviceService.GetGattCharacteristicAsync(Asteroid.NotificationFeedbackCharacteristicUuid);

        //    var result = await this.BackgroundService.RegisterActiveNotificationTask(characteristic);
        //    if (!result)
        //    {
        //        await this.DialogService.ShowMessage("I cannot be able to finish active notification handlers registration!", "Error");
        //    }
        //}

        private async void InizializeScreenshotContentHandlers()
        {
            var result = await this.DeviceService.RegisterToScreenshotContentService();
            if (!result)
            {
                await this.DialogService.ShowMessage("I cannot be able to finish screenshot content handlers registration!", "Error");
            }
        }

        private async void InitializeUserNotificationsHandlers()
        {
            if (!ApiHelper.CheckIfSystemSupportNotificationListener()) return;

            this.BackgroundService.RegisterUserNotificationTask();

            await this.NotificationsService.RetriveNotificationsAsync();
        }

        private async void UpdateBatteryStatus()
        {
            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();

            this.BatteryPercentage = batteryPercentage;
            this.BatteryLevel = BatteryHelper.Parse(batteryPercentage);
        }

        private async void OnBatteryLevelCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var batteryPercentage = BatteryHelper.GetPercentage(args.CharacteristicValue);

                this.BatteryPercentage = batteryPercentage;
                this.BatteryLevel = BatteryHelper.Parse(batteryPercentage);
            });
        }
    }
}
