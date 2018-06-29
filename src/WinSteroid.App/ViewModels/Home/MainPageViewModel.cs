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
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels.Home
{
    public class MainPageViewModel : BaseMainPageViewModel
    {
        private readonly ApplicationsService ApplicationsService;
        private readonly BackgroundService BackgroundService;
        private readonly DeviceService DeviceService;
        private readonly NotificationsService NotificationsService;

        public MainPageViewModel(
            ApplicationsService applicationsService,
            BackgroundService backgroundService,
            DeviceService deviceService,
            NotificationsService notificationsService,
            INavigationService navigationService,
            IDialogService dialogService) : base(dialogService, navigationService)
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

        public override async void Initialize()
        {
            this.IsBusy = true;
            this.BusyMessage = ResourcesHelper.GetLocalizedString("HomeMainInitializingMessage");

            this.DeviceName = this.DeviceService.Current.Name;
            
            var newPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            var oldPercentage = this.BatteryPercentage;

            this.InitializeBatteryLevelHandlersAsync();

            this.BatteryPercentage = newPercentage;
            this.BatteryLevel = BatteryHelper.Parse(newPercentage);

            App.RemoveWelcomePageFromBackStack();

            this.IsBusy = false;
            this.BusyMessage = string.Empty;
        }

        public override void Reset()
        {
            this.BatteryLevel = BatteryLevel.Dead;
            this.BatteryPercentage = 0;
        }

        public override void InitializeMenuOptions()
        {
            if (!this.MenuOptions.IsNullOrEmpty()) return;

            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("HomeMainSettingsItemLabel"), Command = SettingsCommand });
            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("HomeMainScreenshotsItemLabel"), Command = ScreenshotsCommand });        
            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("HomeMainWallpapersItemLabel"), Command = WallpapersCommand });
            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("HomeMainWatchFacesItemLabel"), Command = WatchFacesCommand });
            this.MenuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = ResourcesHelper.GetLocalizedString("HomeMainTutorialsItemLabel"), Command = TutorialsCommand });
        }

        private BatteryLevel _batteryLevel;
        public BatteryLevel BatteryLevel
        {
            get { return _batteryLevel; }
            set { Set(nameof(BatteryLevel), ref _batteryLevel, value); }
        }

        private int _batteryPercentage;
        public int BatteryPercentage
        {
            get { return _batteryPercentage; }
            set { Set(nameof(BatteryPercentage), ref _batteryPercentage, value); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { Set(nameof(DeviceName), ref _deviceName, value); }
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

        private RelayCommand _screenshotsCommand;
        public RelayCommand ScreenshotsCommand
        {
            get
            {
                if (_screenshotsCommand == null)
                {
                    _screenshotsCommand = new RelayCommand(GoToScreenshots);
                }

                return _screenshotsCommand;
            }
        }

        private void GoToScreenshots()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.HomeScreenshots));
        }

        private RelayCommand _wallpapersCommand;
        public RelayCommand WallpapersCommand
        {
            get
            {
                if (_wallpapersCommand == null)
                {
                    _wallpapersCommand = new RelayCommand(GoToWallpapers);
                }

                return _wallpapersCommand;
            }
        }

        private void GoToWallpapers()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.TransfersWallpapers));
        }

        private RelayCommand _watchFacesCommand;
        public RelayCommand WatchFacesCommand
        {
            get
            {
                if (_watchFacesCommand == null)
                {
                    _watchFacesCommand = new RelayCommand(GoToWatchFaces);
                }

                return _watchFacesCommand;
            }
        }

        private void GoToWatchFaces()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.TransfersWatchFace));
        }

        private RelayCommand _tutorialsCommand;
        public RelayCommand TutorialsCommand
        {
            get
            {
                if (_tutorialsCommand == null)
                {
                    _tutorialsCommand = new RelayCommand(GoToTutorials);
                }

                return _tutorialsCommand;
            }
        }

        private void GoToTutorials()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Tutorials));
        }

        private RelayCommand _updateBatteryPercentageCommand;
        public RelayCommand UpdateBatteryPercentageCommand
        {
            get
            {
                if (_updateBatteryPercentageCommand == null)
                {
                    _updateBatteryPercentageCommand = new RelayCommand(UpdateBatteryPercentage);
                }

                return _updateBatteryPercentageCommand;
            }
        }

        private async void UpdateBatteryPercentage()
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                this.IsBusy = true;

                var newPercentage = await this.DeviceService.GetBatteryPercentageAsync();
                var oldPercentage = this.BatteryPercentage;

                this.BatteryPercentage = newPercentage;
                this.BatteryLevel = BatteryHelper.Parse(newPercentage);

                this.IsBusy = false;
            });
        }

        private async Task InizializeScreenshotContentHandlersAsync()
        {
            var result = await this.DeviceService.RegisterToScreenshotContentService();
            if (!result)
            {
                await this.DialogService.ShowMessage(
                    ResourcesHelper.GetLocalizedString("HomeMainScreenshotServiceHandlersRegistrationFailedError"), 
                    ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
            }
        }

        private async void InitializeBatteryLevelHandlersAsync()
        {
            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);
            if (characteristic == null) return;

            var result = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (result != GattCommunicationStatus.Success) return;

            characteristic.ValueChanged += OnBatteryLevelValueChanged;
        }

        private async void OnBatteryLevelValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var newPercentage = BatteryHelper.GetPercentage(args.CharacteristicValue);
                if (newPercentage > 0)
                {
                    TilesHelper.UpdateBatteryTile(newPercentage);
                }
                else
                {
                    TilesHelper.ResetBatteryTile();
                }

                var oldPercentage = this.BatteryPercentage;
                this.BatteryPercentage = newPercentage;
                this.BatteryLevel = BatteryHelper.Parse(newPercentage);
            });
        }
    }
}
