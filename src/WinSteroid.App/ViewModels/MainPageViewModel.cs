using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.App.Helpers;
using WinSteroid.App.Models;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly NotificationsService NotificationsService;
        private readonly IDialogService DialogService;

        public MainPageViewModel(DeviceService deviceService, BackgroundService backgroundService, NotificationsService notificationsService, IDialogService dialogService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.NotificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            this.BatteryTilePinned = TilesHelper.BatteryTileExists();

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

        private bool _batteryTilePinned;
        public bool BatteryTilePinned
        {
            get { return _batteryTilePinned; }
            set { Set(nameof(BatteryTilePinned), ref _batteryTilePinned, value); }
        }

        private RelayCommand _pinBatteryTileCommand;
        public RelayCommand PinBatteryTileCommand
        {
            get
            {
                if (_pinBatteryTileCommand == null)
                {
                    _pinBatteryTileCommand = new RelayCommand(PinBatteryTile);
                }

                return _pinBatteryTileCommand;
            }
        }

        private async void PinBatteryTile()
        {
            var deviceId = this.DeviceService.Current.Id;
            var deviceName = this.DeviceService.Current.Name;
            var result = await TilesHelper.PinBatteryTileAsync(deviceId, deviceName);
            if (result)
            {
                this.BatteryTilePinned = true;
                TilesHelper.UpdateBatteryTile(this.BatteryPercentage);
            }
        }

        private RelayCommand _unpinBatteryTileCommand;
        public RelayCommand UnpinBatteryTileCommand
        {
            get
            {
                if (_unpinBatteryTileCommand == null)
                {
                    _unpinBatteryTileCommand = new RelayCommand(UnpinBatteryTile);
                }

                return _unpinBatteryTileCommand;
            }
        }

        private async void UnpinBatteryTile()
        {
            var result = await TilesHelper.UnpinBatteryTileAsync();
            if (result)
            {
                this.BatteryTilePinned = false;
            }
        }

        private RelayCommand _unregisterTasksCommand;
        public RelayCommand UnregisterTasksCommand
        {
            get
            {
                if (_unregisterTasksCommand == null)
                {
                    _unregisterTasksCommand = new RelayCommand(this.BackgroundService.UnregisterAllTasks);
                }

                return _unregisterTasksCommand;
            }
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
            this.BatteryLevel = BatteryLevelExtensions.Parse(batteryPercentage);
        }

        private async void OnBatteryLevelCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var batteryPercentage = BatteryHelper.GetPercentage(args.CharacteristicValue);

                this.BatteryPercentage = batteryPercentage;
                this.BatteryLevel = BatteryLevelExtensions.Parse(batteryPercentage);
            });
        }
    }
}
