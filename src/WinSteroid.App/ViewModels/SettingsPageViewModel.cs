using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class SettingsPageViewModel : BasePageViewModel
    {
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly NotificationsService NotificationsService;

        public SettingsPageViewModel(
            DeviceService deviceService, 
            BackgroundService backgroundService,
            NotificationsService notificationsService,
            IDialogService dialogService,
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.NotificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));

            this.Initialize();
        }

        public override void Initialize()
        {
            this.EnableUserNotifications = this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName);
            this.UseBatteryLiveTile = TilesHelper.BatteryTileExists();

            this.Initialized = true;
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        private bool _enableUserNotifications;
        public bool EnableUserNotifications
        {
            get { return _enableUserNotifications; }
            set
            {
                if (!this.CanEnableUserNotifications) return;

                if (!Set(nameof(EnableUserNotifications), ref _enableUserNotifications, value)) return;

                if (_enableUserNotifications)
                {
                    this.RegisterUserNotificationTask();
                    return;
                }

                this.BackgroundService.Unregister(BackgroundService.UserNotificationsTaskName);
            }
        }

        public bool CanEnableUserNotifications
        {
            get { return ApiHelper.CheckIfSystemSupportNotificationListener(); }
        }

        private async void RegisterUserNotificationTask()
        {
            var accessResult = await this.NotificationsService.RequestAccessAsync();
            if (!accessResult)
            {
                this.EnableUserNotifications = false;
                return;
            }

            var result = this.BackgroundService.RegisterUserNotificationTask();
            if (result) return;

            this.EnableUserNotifications = false;
        }

        private bool _useBatteryLiveTile;
        public bool UseBatteryLiveTile
        {
            get { return _useBatteryLiveTile; }
            set
            {
                if (!Set(nameof(UseBatteryLiveTile), ref _useBatteryLiveTile, value)) return;

                if (_useBatteryLiveTile)
                {
                    this.PinBatteryTile();
                    return;
                }

                this.UnpinBatteryTile();
            }
        }

        private async void PinBatteryTile()
        {
            var deviceId = this.DeviceService.Current.Id;
            var deviceName = this.DeviceService.Current.Name;
            var result = await TilesHelper.PinBatteryTileAsync(deviceId, deviceName);
            if (!result)
            {
                this.UseBatteryLiveTile = false;
                return;
            }

            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);
            
            await this.BackgroundService.RegisterBatteryLevelTask(characteristic);

            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            TilesHelper.UpdateBatteryTile(batteryPercentage);
        }

        private async void UnpinBatteryTile()
        {
            var result = await TilesHelper.UnpinBatteryTileAsync();
            if (result)
            {
                this.BackgroundService.Unregister(BackgroundService.BatteryLevelTaskName);
                return;
            }
            
            this.UseBatteryLiveTile = true;
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

        private RelayCommand _iconsCommand;
        public RelayCommand IconsCommand
        {
            get
            {
                if (_iconsCommand == null)
                {
                    _iconsCommand = new RelayCommand(GoToIcons);
                }

                return _iconsCommand;
            }
        }

        private void GoToIcons()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Icons));
        }
    }
}
