using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.Services;

namespace WinSteroid.App
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(Views.WelcomePage), e.Arguments);
                }

                Window.Current.Activate();
            }
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

            var notificationService = new NotificationsService(new Data.Database());
            var deviceService = new DeviceService();

            if (string.Equals(args.TaskInstance.Task.Name, BackgroundService.UserNotificationsTaskName, StringComparison.OrdinalIgnoreCase))
            {
                var userNotifications = await notificationService.RetriveNotificationsAsync();
                if (!userNotifications.Any()) return;

                foreach (var userNotification in userNotifications)
                {
                    await deviceService.InsertNotificationAsync(userNotification);
                }

                var currentIds = userNotifications.Select(n => n.Id.ToString()).ToArray();
                var removedNotificationIds = await notificationService.GetRemovedNotificationsIdsAsync(currentIds);
                if (removedNotificationIds.Count > 0)
                {
                    foreach (var id in removedNotificationIds)
                    {
                        await notificationService.RemoveNotificationAsync(id);
                        await deviceService.RemoveNotificationAsync(id);
                    }
                }
            }

            backgroundTaskDeferral.Complete();
        }
    }
}
