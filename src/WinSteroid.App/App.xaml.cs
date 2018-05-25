using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.Services;
using WinSteroid.App.ViewModels;

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

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
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

            if (!StringExtensions.OrdinalIgnoreCaseEquals(args.TaskInstance.Task.Name, BackgroundService.UserNotificationsTaskName))
            {
                backgroundTaskDeferral.Complete();
                return;
            }

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
                await deviceService.ConnectAsync(deviceId);
            }

            var userNotifications = (await notificationService.RetriveNotificationsAsync())?.ToArray() ?? new UserNotification[0];
            if (userNotifications.Length == 0)
            {
                notificationService.SaveLastNotificationIds(new string[0]);
                backgroundTaskDeferral.Complete();
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

            backgroundTaskDeferral.Complete();
        }
    }
}
