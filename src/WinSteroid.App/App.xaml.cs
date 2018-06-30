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

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.HockeyApp;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.Services;
using WinSteroid.App.ViewModels;
using WinSteroid.Common.Background;
using WinSteroid.Common.Bluetooth;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Notifications;

namespace WinSteroid.App
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            HockeyClient.Current.ConfigureWithDefaultParameters();
        }
        
        public static bool InDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        private void UpdateResourcesDictionaries()
        {
            if (ApiHelper.IsMobileSystem())
            {
                this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx:///Themes/MobileDictionary.xaml") });
            }

            if (ApiHelper.SupportsAcrylicBrushes())
            {
                this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("ms-appx:///Themes/FluentDictionary.xaml") });
            }
        }

        public static void RemoveWelcomePageFromBackStack()
        {
            if (!(Window.Current.Content is Frame rootFrame)) return;

            var welcomePageStackEntries = rootFrame.BackStack.Where(p => p.SourcePageType == typeof(Views.Home.WelcomePage)).ToArray();
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

            rootFrame.Navigate(typeof(Views.Home.WelcomePage), arguments);
        }

        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Window.Current.Content is Frame rootFrame && rootFrame.CanGoBack)
            {
                e.Handled = true;

                var navigationService = SimpleIoc.Default.GetInstance<INavigationService>();
                var viewModel = ViewModelLocator.GetCurrentViewModel(navigationService.CurrentPageKey);
                if (await viewModel.CanGoBack())
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
                case BackgroundManager.TimeBatteryLevelTaskName:
                    {
                        await this.OnTimeBatteryLevelBackgroundTaskActivated(args.TaskInstance);
                        break;
                    }
                case BackgroundManager.UserNotificationsTaskName:
                    {
                        await this.OnUserNotificationsBackgroundTaskActivated(args.TaskInstance);
                        break;
                    }
            }

            backgroundTaskDeferral.Complete();
        }

        private async Task OnTimeBatteryLevelBackgroundTaskActivated(IBackgroundTaskInstance backgroundTaskInstance)
        {
            if (!TilesHelper.BatteryTileExists()) return;

            var applicationsService = new ApplicationsService();

            var errorMessage = await DeviceManager.ConnectAsync();
            if (!string.IsNullOrWhiteSpace(errorMessage)) return;

            var percentage = await DeviceManager.GetBatteryPercentageAsync();
            if (percentage > 0)
            {
                TilesHelper.UpdateBatteryTile(percentage);
            }
            else
            {
                TilesHelper.ResetBatteryTile();
            }
        }

        private async Task OnUserNotificationsBackgroundTaskActivated(IBackgroundTaskInstance backgroundTaskInstance)
        {
            var applicationsService = new ApplicationsService();

            var errorMessage = await DeviceManager.ConnectAsync();
            if (!string.IsNullOrWhiteSpace(errorMessage)) return;

            var userNotifications = await NotificationsManager.RetriveNotificationsAsync();
            if (userNotifications.IsNullOrEmpty()) return;

            foreach (var userNotification in userNotifications)
            {
                var application = applicationsService.GetApplicationPreferenceByAppId(userNotification.AppInfo.PackageFamilyName);
                await DeviceManager.InsertNotificationAsync(userNotification, application);
            }
            
            await applicationsService.UpsertFoundApplicationsAsync(userNotifications);

            await DeviceManager.DisconnectAsync();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                this.UpdateResourcesDictionaries();

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
                    rootFrame.Navigate(typeof(Views.Home.WelcomePage), e.Arguments);
                }

                Window.Current.Activate();
            }

            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
        }
    }
}
