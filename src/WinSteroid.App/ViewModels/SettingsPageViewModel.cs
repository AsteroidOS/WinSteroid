using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly INavigationService NavigationService;

        public SettingsPageViewModel(DeviceService deviceService, BackgroundService backgroundService, INavigationService navigationService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            this.EnableUserNotifications = backgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName);
            this.UseBatteryLiveTile = TilesHelper.BatteryTileExists();
        }

        private bool _enableUserNotifications;
        public bool EnableUserNotifications
        {
            get { return _enableUserNotifications; }
            set
            {
                if (!Set(nameof(EnableUserNotifications), ref _enableUserNotifications, value)) return;

                if (_enableUserNotifications)
                {
                    this.BackgroundService.RegisterUserNotificationTask();
                }
                else
                {
                    this.BackgroundService.Unregister(BackgroundService.UserNotificationsTaskName);
                }
            }
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
                }
                else
                {
                    this.UnpinBatteryTile();
                }
            }
        }

        private async void PinBatteryTile()
        {
            var deviceId = this.DeviceService.Current.Id;
            var deviceName = this.DeviceService.Current.Name;
            var result = await TilesHelper.PinBatteryTileAsync(deviceId, deviceName);
            if (!result)
            {
                this.EnableUserNotifications = false;
                return;
            }
            
            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            TilesHelper.UpdateBatteryTile(batteryPercentage);
        }

        private async void UnpinBatteryTile()
        {
            var result = await TilesHelper.UnpinBatteryTileAsync();
            if (result) return;
            
            this.EnableUserNotifications = true;
        }

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

        private void GoBack()
        {
            this.NavigationService.GoBack();
        }

        private RelayCommand _exportApplicationsFileCommand;
        public RelayCommand ExportApplicationsFileCommand
        {
            get
            {
                if (_exportApplicationsFileCommand == null)
                {
                    _exportApplicationsFileCommand = new RelayCommand(ExportData);
                }

                return _exportApplicationsFileCommand;
            }
        }

        private async void ExportData()
        {
            await ApplicationsHelper.ExportDataAsync();
        }
    }
}
