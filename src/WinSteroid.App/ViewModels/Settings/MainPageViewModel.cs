//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WinSteroid.App.Services;
using WinSteroid.Common;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels.Settings
{
    public class MainPageViewModel : BaseMainPageViewModel
    {
        private readonly ApplicationsService ApplicationsService;
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly NotificationsService NotificationsService;

        public MainPageViewModel(
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
            this.CustomDate = DateTimeOffset.Now;
            this.CustomTime = DateTimeOffset.Now.TimeOfDay;
            this.DeviceName = this.DeviceService.Current.Name;
            this.EnableUserNotifications = this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName);

            var lastSavedBatteryTaskFrequency = SettingsHelper.GetValue(Constants.LastSavedBatteryTaskFrequencySettingKey, (uint)15);
            this.BatteryCheckFrequency = this.AvailableBatteryCheckFrequencies.FirstOrDefault(bf => bf.Minutes == lastSavedBatteryTaskFrequency);

            this.UseBatteryLiveTile = TilesHelper.BatteryTileExists();
        }

        public override void InitializeMenuOptions()
        {
            if (!this.MenuOptions.IsNullOrEmpty()) return;

            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("SettingsMainHomeItemLabel"), Command = HomeCommand });
            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("SettingsMainAboutItemLabel"), Command = AboutCommand });
            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("SettingsMainApplicationsItemLabel"), Command = ApplicationsCommand });
        }

        public override void Reset()
        {
            this.DeviceName = string.Empty;
            this.UseBatteryLiveTile = false;
            this.EnableUserNotifications = false;
        }

        private string _applicationName;
        public string ApplicationName
        {
            get { return _applicationName; }
            set { Set(nameof(ApplicationName), ref _applicationName, value); }
        }

        private List<BatteryFrequency> _availableBatteryCheckFrequencies;
        public List<BatteryFrequency> AvailableBatteryCheckFrequencies
        {
            get
            {
                if (_availableBatteryCheckFrequencies == null)
                {
                    var labelFormat = ResourcesHelper.GetLocalizedString("SettingsMainBatteryCheckFrequencyMinutesFormat");

                    _availableBatteryCheckFrequencies = new List<BatteryFrequency>
                    {
                        new BatteryFrequency { Label = string.Format(labelFormat, 15), Minutes = 15 },
                        new BatteryFrequency { Label = string.Format(labelFormat, 30), Minutes = 30 },
                        new BatteryFrequency { Label = string.Format(labelFormat, 60), Minutes = 60 },
                        //new BatteryFrequency { Label = "Always", Minutes = 0 }
                    };
                }

                return _availableBatteryCheckFrequencies;
            }
        }

        private BatteryFrequency _batteryCheckFrequency;
        public BatteryFrequency BatteryCheckFrequency
        {
            get { return _batteryCheckFrequency; }
            set
            {
                if (!Set(nameof(BatteryCheckFrequency), ref _batteryCheckFrequency, value)) return;

                if (_batteryCheckFrequency == null) return;

                if (!TilesHelper.BatteryTileExists()) return;

                this.BackgroundService.Unregister(BackgroundService.BatteryLevelTaskName);
                this.BackgroundService.Unregister(BackgroundService.TimeBatteryLevelTaskName);

                SettingsHelper.SetValue(Constants.LastSavedBatteryTaskFrequencySettingKey, _batteryCheckFrequency.Minutes);

                this.RegisterBatteryTask();
            }
        }

        private bool _showBatteryCheckWarning;
        public bool ShowBatteryCheckWarning
        {
            get { return _showBatteryCheckWarning; }
            private set { Set(nameof(ShowBatteryCheckWarning), ref _showBatteryCheckWarning, value); }
        }

        public bool CanEnableUserNotifications
        {
            get { return ApiHelper.IsNotificationListenerSupported(); }
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
                if (!Set(nameof(EnableUserNotifications), ref _enableUserNotifications, value)) return;

                this.ManageUserNotificationSelection();
            }
        }

        private void ManageUserNotificationSelection()
        {
            if (this.EnableUserNotifications)
            {
                this.RegisterUserNotificationTask();
                return;
            }

            this.BackgroundService.Unregister(BackgroundService.UserNotificationsTaskName);
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

        private bool _scpCredentialsRemoved;
        public bool ScpCredentialsRemoved
        {
            get { return _scpCredentialsRemoved; }
            set { Set(nameof(ScpCredentialsRemoved), ref _scpCredentialsRemoved, value); }
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

                this.ManageBatteryLiveTileSelection();
            }
        }

        private void ManageBatteryLiveTileSelection()
        {
            if (this.UseBatteryLiveTile)
            {
                this.PinBatteryTile();
                return;
            }

            this.UnpinBatteryTile();
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

            this.RegisterBatteryTask();

            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            TilesHelper.UpdateBatteryTile(batteryPercentage);
        }

        private async void RegisterBatteryTask()
        {
            this.ShowBatteryCheckWarning = this.BatteryCheckFrequency.Minutes < 15;

            if (this.BatteryCheckFrequency.Minutes < 15)
            {
                await this.BackgroundService.RegisterBatteryLevelTask();
            }
            else
            {
                await this.BackgroundService.RegisterTimeBatteryLevelTask(this.BatteryCheckFrequency.Minutes);
            }
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
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.SettingsApplications));
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
                message: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationMessage"),
                title: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationTitle"),
                buttonConfirmText: ResourcesHelper.GetLocalizedString("SharedYesMessage"),
                buttonCancelText: ResourcesHelper.GetLocalizedString("SharedNoMessage"),
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

            await this.DialogService.ShowError(
                ResourcesHelper.GetLocalizedString("SettingsMainSetTimeErrorMessage"), 
                ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
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

            await this.DialogService.ShowError(
                ResourcesHelper.GetLocalizedString("SettingsMainSetTimeErrorMessage"), 
                ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
        }

        private RelayCommand _aboutCommand;
        public RelayCommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new RelayCommand(GoToAbout);
                }

                return _aboutCommand;
            }
        }

        private void GoToAbout()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.SettingsAbout));
        }

        private RelayCommand _homeCommand;
        public RelayCommand HomeCommand
        {
            get
            {
                if (_homeCommand == null)
                {
                    _homeCommand = new RelayCommand(GoToHome);
                }

                return _homeCommand;
            }
        }

        private void GoToHome()
        {
            this.NavigationService.GoBack();
        }

        private RelayCommand _resetScpCredentialsCommand;
        public RelayCommand ResetScpCredentialsCommand
        {
            get
            {
                if (_resetScpCredentialsCommand == null)
                {
                    _resetScpCredentialsCommand = new RelayCommand(ResetScpCredentials);
                }

                return _resetScpCredentialsCommand;
            }
        }

        private void ResetScpCredentials()
        {
            this.ScpCredentialsRemoved = false;
            SettingsHelper.RemoveAllScpCredentials();
            this.ScpCredentialsRemoved = true;
        }

        private async void ManageResetMessageResult(bool confirmed)
        {
            if (!confirmed) return;

            await this.DialogService.ShowMessage(
                message: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationKeepIconPreferencesMessage"),
                title: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationKeepIconPreferencesTitle"),
                buttonConfirmText: ResourcesHelper.GetLocalizedString("SharedYesMessage"),
                buttonCancelText: ResourcesHelper.GetLocalizedString("SharedNoMessage"),
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
                message: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationExportDataMessage"),
                title: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationExportDataTitle"),
                buttonConfirmText: ResourcesHelper.GetLocalizedString("SharedYesMessage"),
                buttonCancelText: ResourcesHelper.GetLocalizedString("SharedNoMessage"),
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
                        message: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationExportDataFailedOrCanceledMessage"),
                        title: ResourcesHelper.GetLocalizedString("SettingsMainResetApplicationExportDataTitle"),
                        buttonConfirmText: ResourcesHelper.GetLocalizedString("SharedYesMessage"),
                        buttonCancelText: ResourcesHelper.GetLocalizedString("SharedNoMessage"),
                        afterHideCallback: ManageExportIconPreferencesMessageResult);
                    return;
                }
            }

            ViewModelLocator.Home.Reset();
            ViewModelLocator.HomeWelcome.Reset();
            await this.DeviceService.DisconnectAsync();
            App.Reset();
        }
    }

    public class BatteryFrequency
    {
        public uint Minutes { get; set; }

        public string Label { get; set; }
    }
}
