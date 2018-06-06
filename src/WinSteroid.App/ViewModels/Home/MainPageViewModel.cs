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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml.Media.Imaging;
using WinSteroid.App.Messeges;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels.Home
{
    public class MainPageViewModel : BasePageViewModel
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
            this.BusyMessage = "Initializing";

            this.DeviceName = this.DeviceService.Current.Name;
            this.DeviceService.AttachConnectionStatusChangedHandler(OnConnectionStatusChanged);

            this.Notifications = new ObservableCollection<NotificationItemViewModel>();

            var newPercentage = await this.DeviceService.GetBatteryPercentageAsync();
            var oldPercentage = this.BatteryPercentage;

            this.BatteryPercentage = newPercentage;
            this.BatteryLevel = BatteryHelper.Parse(newPercentage);

            this.IsBusy = false;
            this.BusyMessage = string.Empty;

            this.MessengerInstance.Send(BatteryPercentageMessage.Create(newPercentage, oldPercentage), nameof(ViewModelLocator.Home));

            App.RemoveWelcomePageFromBackStack();

            this.Initialized = true;
        }

        public override void Reset()
        {
            this.BatteryLevel = BatteryLevel.Dead;
            this.BatteryPercentage = 0;
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

        private bool _isDeviceConnected;
        public bool IsDeviceConnected
        {
            get { return _isDeviceConnected; }
            set { Set(nameof(IsDeviceConnected), ref _isDeviceConnected, value); }
        }

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set { Set(nameof(IsMenuOpen), ref _isMenuOpen, value); }
        }

        private bool _showNotificationsList;
        public bool ShowNotificationsList
        {
            get { return _showNotificationsList; }
            set { Set(nameof(ShowNotificationsList), ref _showNotificationsList, value); }
        }

        private void ManageShowNotificationsListSelection()
        {
            if (this.ShowNotificationsList)
            {
                this.RegisterNotificationsHandlers();
                return;
            }

            this.UnregisterNotificationsHandlers();
        }

        public List<MenuOptionViewModel> MenuOptions
        {
            get
            {
                var menuOptions = new List<MenuOptionViewModel>
                {
                    new MenuOptionViewModel { Glyph = "", Label = "Settings", Command = SettingsCommand },
                };

                if (!ApiHelper.CheckIfIsSystemMobile())
                {
                    menuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = "Wallpapers", Command = WallpapersCommand });
                    menuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = "WatchFaces", Command = WatchFacesCommand });
                }

                menuOptions.Add(new MenuOptionViewModel { Glyph = "", Label = "Tutorials", Command = TutorialsCommand });

                return menuOptions;
            }
        }

        public void ManageSelectedMenuOption(MenuOptionViewModel menuOption)
        {
            if (menuOption == null || menuOption.Command == null || !menuOption.Command.CanExecute(null)) return;

            menuOption.Command.Execute(null);

            this.IsMenuOpen = false;
        }

        private ObservableCollection<NotificationItemViewModel> _notifications;
        public ObservableCollection<NotificationItemViewModel> Notifications
        {
            get { return _notifications; }
            set { Set(nameof(Notifications), ref _notifications, value); }
        }

        private RelayCommand _menuCommand;
        public RelayCommand MenuCommand
        {
            get
            {
                if (_menuCommand == null)
                {
                    _menuCommand = new RelayCommand(ToggleMenu);
                }

                return _menuCommand;
            }
        }

        private void ToggleMenu()
        {
            this.IsMenuOpen = !this.IsMenuOpen;
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

        private void RegisterNotificationsHandlers()
        {
            if (!this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName)) return;

            this.NotificationsService.RegisterNotificationsChangedHandler(OnNotificationChanged);
        }

        private void UnregisterNotificationsHandlers()
        {
            this.NotificationsService.UnregisterNotificationsChangedHandler();
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

        private async void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            if (!this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName)) return;

            var notifications = await this.NotificationsService.RetriveNotificationsAsync();
            if (notifications.IsNullOrEmpty())
            {
                this.Notifications.Clear();
                return;
            }

            foreach (var notification in this.Notifications)
            {
                if (notifications.All(n => n.Id.ToString() != notification.Id))
                {
                    this.Notifications.Remove(notification);
                }
            }

            foreach (var notification in notifications)
            {
                var existingNotification = this.Notifications.Any(n => n.Id == notification.Id.ToString());
                if (existingNotification) return;

                var application = this.ApplicationsService.GetApplicationPreferenceByAppId(notification.AppInfo.PackageFamilyName);

                var packageIcon = await ImageHelper.ConvertToImageAsync(notification.AppInfo.DisplayInfo);

                this.Notifications.Add(new NotificationItemViewModel
                {
                    Id = notification.Id.ToString(),
                    AppId = notification.AppInfo.PackageFamilyName,
                    PackageName = notification.AppInfo.DisplayInfo.DisplayName,
                    Title = notification.GetTitle(),
                    Body = notification.GetBody(),
                    //LaunchUri = notification.GetLaunchUri(),
                    Icon = (application?.Icon ?? ApplicationIcon.Alert).GetGlyph(),
                    PackageIcon = packageIcon
                });
            }
        }

        public void UpdateNotificationsOptions()
        {
            this.ShowNotificationsList = this.BackgroundService.IsBackgroundTaskRegistered(BackgroundService.UserNotificationsTaskName);
        }
    }
}
