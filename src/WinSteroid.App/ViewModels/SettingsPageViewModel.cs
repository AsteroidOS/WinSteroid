using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class SettingsPageViewModel : BasePageViewModel
    {
        private readonly ApplicationsService ApplicationsService;
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly NotificationsService NotificationsService;

        public SettingsPageViewModel(
            ApplicationsService applicationsService,
            DeviceService deviceService, 
            BackgroundService backgroundService,
            NotificationsService notificationsService,
            IDialogService dialogService,
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.ApplicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.NotificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));

            this.Initialize();
        }

        public override void Initialize()
        {
            this.DeviceName = this.DeviceService.Current.Name;
            this.ApplicationName = Package.Current.DisplayName;
            this.ApplicationFooter = $"{Package.Current.DisplayName} - {Package.Current.Id.GetVersion()}";
            this.EnableUserNotifications = this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName);
            this.UseBatteryLiveTile = TilesHelper.BatteryTileExists();

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

        private string _applicationFooter;
        public string ApplicationFooter
        {
            get { return _applicationFooter; }
            set { Set(nameof(ApplicationFooter), ref _applicationFooter, value); }
        }

        private string _applicationName;
        public string ApplicationName
        {
            get { return _applicationName; }
            set { Set(nameof(ApplicationName), ref _applicationName, value); }
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

            ViewModelLocator.Main.RegisterBatteryLevelHandler();
        }

        private async void UnpinBatteryTile()
        {
            var result = await TilesHelper.UnpinBatteryTileAsync();
            if (!result)
            {
                this.UseBatteryLiveTile = true;
                return;
            }

            this.BackgroundService.Unregister(BackgroundService.BatteryLevelTaskName);

            ViewModelLocator.Main.UnregisterBatteryLevelHandler();
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

        private RelayCommand _resetApplicationCommand;
        public RelayCommand ResetApplicationCommand
        {
            get
            {
                if (_resetApplicationCommand == null)
                {
                    _resetApplicationCommand = new RelayCommand(ResetApplication);
                }

                return _resetApplicationCommand;
            }
        }

        private async void ResetApplication()
        {
            await this.DialogService.ShowMessage(
                message: "Are you sure do you want to disconnect your device and reset the application?",
                title: $"Reset {this.ApplicationName}",
                buttonConfirmText: "Yes, I'm sure",
                buttonCancelText: "Mmm nope",
                afterHideCallback: ManageResetMessageResult);
        }

        private async void ManageResetMessageResult(bool confirmed)
        {
            if (!confirmed) return;

            await this.DialogService.ShowMessage(
                message: "Do you want to keep the current icon preferences for another device?",
                title: "Keep icons preferences",
                buttonConfirmText: "Yes",
                buttonCancelText: "No",
                afterHideCallback: ManageKeepPreferencesMessageResult);
        }

        private async void ManageKeepPreferencesMessageResult(bool keepIconsPreferences)
        {
            this.DeviceName = string.Empty;
            this.UseBatteryLiveTile = false;
            this.EnableUserNotifications = false;

            if (!keepIconsPreferences)
            {
                await this.ApplicationsService.DeleteUserIcons();
                App.Reset();
            }

            await this.DialogService.ShowMessage(
                message: $"Do you want to save a copy of icon preferences as file (to be imported in other {this.ApplicationName} installations?",
                title: "Export data",
                buttonConfirmText: "Yes",
                buttonCancelText: "No",
                afterHideCallback: ManageExportIconPreferencesMessageResult);
        }

        private async void ManageExportIconPreferencesMessageResult(bool exportFile)
        {
            if (exportFile)
            {
                var result = await this.ApplicationsService.ExportDataAsync();
                if (!result)
                {
                    await this.DialogService.ShowMessage(
                        message: $"Export failed or canceled. Do you want to retry?",
                        title: "Export data",
                        buttonConfirmText: "Yes",
                        buttonCancelText: "No",
                        afterHideCallback: ManageExportIconPreferencesMessageResult);
                    return;
                }
            }

            ViewModelLocator.Main.Reset();
            ViewModelLocator.Welcome.Reset();
            await this.DeviceService.DisconnectAsync();
            App.Reset();
        }
    }
}
