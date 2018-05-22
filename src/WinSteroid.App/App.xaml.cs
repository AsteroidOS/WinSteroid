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

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!ApplicationsHelper.Initialized())
            {
                await ApplicationsHelper.InitializeAsync();
            }

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

            var notificationService = new NotificationsService();
            var deviceService = new DeviceService();

            if (!ApplicationsHelper.Initialized())
            {
                await ApplicationsHelper.InitializeAsync();
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

            //LOG APPS FOR ICONS MANAGEMENT
            await ApplicationsHelper.UpsertFoundApplicationsAsync(userNotifications);

            backgroundTaskDeferral.Complete();
        }
    }
}
