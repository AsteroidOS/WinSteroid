﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.Services;
using WinSteroid.App.ViewModels;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        private bool IsRunning { get; set; }

        public static void RemoveWelcomePageFromBackStack()
        {
            if (!(Window.Current.Content is Frame rootFrame)) return;

            var welcomePageStackEntries = rootFrame.BackStack.Where(p => p.SourcePageType == typeof(Views.WelcomePage)).ToArray();
            if (welcomePageStackEntries == null || welcomePageStackEntries.Length == 0) return;

            foreach (var pageStackEntry in welcomePageStackEntries)
            {
                rootFrame.BackStack.Remove(pageStackEntry);
            }
        }

        public static void Reset(string arguments = null)
        {
            if (!(Window.Current.Content is Frame rootFrame)) return;

            rootFrame.BackStack.Clear();

            rootFrame.Navigate(typeof(Views.WelcomePage), arguments);
        }

        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Window.Current.Content is Frame rootFrame && rootFrame.CanGoBack)
            {
                e.Handled = true;

                var navigationService = SimpleIoc.Default.GetInstance<INavigationService>();
                var viewModel = ViewModelLocator.GetCurrentViewModel(navigationService.CurrentPageKey);
                var canGoBack = await viewModel.CanGoBack();
                if (canGoBack)
                {
                    rootFrame.GoBack();
                }
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().UpdateAppViewBackButtonVisibility((Frame)sender);
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var backgroundTaskDeferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case BackgroundService.ActiveNotificationTaskName:
                    {
                        await this.OnActiveNotificationBackgroundTaskActivated(args.TaskInstance);
                        break;
                    }
                case BackgroundService.BatteryLevelTaskName:
                    {
                        await this.OnBatteryLevelBackgroundTaskActivated(args.TaskInstance);
                        break;
                    }
                case BackgroundService.SystemSessionTaskName:
                    {
                        await this.OnSystemSessionBackgroundTaskActivated(args.TaskInstance);
#if DEBUG
                        ToastsHelper.Show("Battery task re-registered!");
#endif
                        break;
                    }
                case BackgroundService.UserNotificationsTaskName:
                    {
                        await this.OnUserNotificationsBackgroundTaskActivated(args.TaskInstance);
                        break;
                    }
            }

            backgroundTaskDeferral.Complete();
        }

        private Task OnActiveNotificationBackgroundTaskActivated(IBackgroundTaskInstance backgroundTaskInstance)
        {
            //var notificationService = this.IsRunning ? SimpleIoc.Default.GetInstance<NotificationsService>() : new NotificationsService();

            //var details = (GattCharacteristicNotificationTriggerDetails)backgroundTaskInstance.TriggerDetails;

            //notificationService.ManageNotificationAction(details.Value);

            return Task.CompletedTask;
        }

        private Task OnBatteryLevelBackgroundTaskActivated(IBackgroundTaskInstance backgroundTaskInstance)
        {
            var details = (GattCharacteristicNotificationTriggerDetails)backgroundTaskInstance.TriggerDetails;

            var percentage = BatteryHelper.GetPercentage(details.Value);
            if (percentage > 0)
            {
                TilesHelper.UpdateBatteryTile(percentage);
            }
            else
            {
                TilesHelper.ResetBatteryTile();
            }

            if (this.IsRunning)
            {
                ViewModelLocator.Main.BatteryPercentage = percentage;
            }

            return Task.CompletedTask;
        }

        private Task OnSystemSessionBackgroundTaskActivated(IBackgroundTaskInstance backgroundTaskInstance)
        {
            if (!TilesHelper.BatteryTileExists()) return Task.CompletedTask;

            ApplicationsService applicationsService = null;
            DeviceService deviceService = null;
            BackgroundService backgroundService = null;

            if (this.IsRunning)
            {
                applicationsService = SimpleIoc.Default.GetInstance<ApplicationsService>();
                deviceService = SimpleIoc.Default.GetInstance<DeviceService>();
                backgroundService = SimpleIoc.Default.GetInstance<BackgroundService>();
            }
            else
            {
                applicationsService = new ApplicationsService();
                deviceService = new DeviceService(applicationsService);
                backgroundService = new BackgroundService(deviceService);
            }

            backgroundService.Unregister(BackgroundService.BatteryLevelTaskName);
            return backgroundService.RegisterBatteryLevelTask();
        }

        private async Task OnUserNotificationsBackgroundTaskActivated(IBackgroundTaskInstance backgroundTaskInstance)
        {
            ApplicationsService applicationsService = null;
            NotificationsService notificationService = null;
            DeviceService deviceService = null;

            if (this.IsRunning)
            {
                applicationsService = SimpleIoc.Default.GetInstance<ApplicationsService>();
                notificationService = SimpleIoc.Default.GetInstance<NotificationsService>();
                deviceService = SimpleIoc.Default.GetInstance<DeviceService>();
            }
            else
            {
                applicationsService = new ApplicationsService();
                notificationService = new NotificationsService();
                deviceService = new DeviceService(applicationsService);
            }

            if (deviceService.BluetoothDevice == null || deviceService.Current == null)
            {
                var deviceId = deviceService.GetLastSavedDeviceId();
                var connected = await deviceService.ConnectAsync(deviceId);
                if (!connected) return;
            }

            var userNotifications = (await notificationService.RetriveNotificationsAsync())?.ToArray() ?? new UserNotification[0];
            if (userNotifications.Length == 0)
            {
                notificationService.SaveLastNotificationIds(new string[0]);
                return;
            }

            var lastNotificationIds = notificationService.GetLastNotificationIds();
            if (lastNotificationIds.Count > 0)
            {
                var removedNotificationIds = lastNotificationIds
                    .Where(id => userNotifications.All(notification => !StringExtensions.OrdinalIgnoreCaseEquals(notification.Id.ToString(), id)))
                    .ToArray();

                if (removedNotificationIds?.Length > 0)
                {
                    foreach (var notificationId in removedNotificationIds)
                    {
                        await deviceService.RemoveNotificationAsync(notificationId);
                    }
                }

                userNotifications = userNotifications
                    .Where(notification => lastNotificationIds.All(id => !StringExtensions.OrdinalIgnoreCaseEquals(notification.Id.ToString(), id)))
                    .ToArray();
            }

            foreach (var userNotification in userNotifications)
            {
                await deviceService.InsertNotificationAsync(userNotification);
            }

            notificationService.SaveLastNotificationIds(userNotifications);

            await applicationsService.UpsertFoundApplicationsAsync(userNotifications);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                Window.Current.Content = rootFrame;

                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                SystemNavigationManager.GetForCurrentView().UpdateAppViewBackButtonVisibility(rootFrame);
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(Views.WelcomePage), e.Arguments);
                }

                Window.Current.Activate();
            }

            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
            this.IsRunning = true;
        }
    }
}
