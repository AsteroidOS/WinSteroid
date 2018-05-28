﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            this.ApplicationName = Package.Current.DisplayName;
            this.ApplicationFooter = $"{Package.Current.DisplayName} - {Package.Current.Id.GetVersion()}";
            this.CustomDate = DateTimeOffset.Now;
            this.CustomTime = DateTimeOffset.Now.TimeOfDay;
            this.DeviceName = this.DeviceService.Current.Name;
            this.EnableUserNotifications = this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName);
            this.UseBatteryLiveTile = TilesHelper.BatteryTileExists();

            this.Initialized = true;
        }

        public override void Reset()
        {
            this.DeviceName = string.Empty;
            this.UseBatteryLiveTile = false;
            this.EnableUserNotifications = false;
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

        public bool CanEnableUserNotifications
        {
            get { return ApiHelper.CheckIfSystemSupportNotificationListener(); }
        }

        private DateTimeOffset _customDate;
        public DateTimeOffset CustomDate
        {
            get { return _customDate; }
            set { Set(nameof(CustomDate), ref _customDate, value); }
        }

        private TimeSpan _customTime;
        public TimeSpan CustomTime
        {
            get { return _customTime; }
            set { Set(nameof(CustomTime), ref _customTime, value); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { Set(nameof(DeviceName), ref _deviceName, value); }
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

        private bool _showCustomDateTimeOptions;
        public bool ShowCustomDateTimeOptions
        {
            get { return _showCustomDateTimeOptions; }
            set { Set(nameof(ShowCustomDateTimeOptions), ref _showCustomDateTimeOptions, value); }
        }

        private bool _timeSetSuccessfully;
        public bool TimeSetSuccessfully
        {
            get { return _timeSetSuccessfully; }
            set { Set(nameof(TimeSetSuccessfully), ref _timeSetSuccessfully, value); }
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
            
            await this.BackgroundService.RegisterBatteryLevelTask();

            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            TilesHelper.UpdateBatteryTile(batteryPercentage);
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
        }

        private RelayCommand _applicationsCommand;
        public RelayCommand ApplicationsCommand
        {
            get
            {
                if (_applicationsCommand == null)
                {
                    _applicationsCommand = new RelayCommand(GoToApplications);
                }

                return _applicationsCommand;
            }
        }

        private void GoToApplications()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Applications));
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

        private RelayCommand _syncDateCommand;
        public RelayCommand SyncDateCommand
        {
            get
            {
                if (_syncDateCommand == null)
                {
                    _syncDateCommand = new RelayCommand(SyncDate);
                }

                return _syncDateCommand;
            }
        }

        private async void SyncDate()
        {
            var dateSynced = await this.DeviceService.SetTimeAsync(DateTime.Now);
            this.TimeSetSuccessfully = dateSynced;

            if (dateSynced) return;

            await this.DialogService.ShowError("I cannot be able to automatically set time on your device", "Error");
        }

        private RelayCommand _manuallySyncDateCommand;
        public RelayCommand ManuallySyncDateCommand
        {
            get
            {
                if (_manuallySyncDateCommand == null)
                {
                    _manuallySyncDateCommand = new RelayCommand(ManuallySyncDate);
                }

                return _manuallySyncDateCommand;
            }
        }

        private async void ManuallySyncDate()
        {
            var fullDateTime = this.CustomDate.Date.Add(this.CustomTime);

            var dateSynced = await this.DeviceService.SetTimeAsync(fullDateTime);
            this.TimeSetSuccessfully = dateSynced;

            if (dateSynced) return;

            await this.DialogService.ShowError("I cannot be able to manually set time on your device", "Error");
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
            this.Reset();

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
