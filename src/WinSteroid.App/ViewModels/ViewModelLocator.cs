﻿//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
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

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using WinSteroid.Common.Background;

namespace WinSteroid.App.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<Home.WelcomePageViewModel>();
            SimpleIoc.Default.Register<Home.MainPageViewModel>();
            SimpleIoc.Default.Register<Home.ScreenshotsPageViewModel>();

            SimpleIoc.Default.Register<Settings.AboutPageViewModel>();
            SimpleIoc.Default.Register<Settings.ApplicationPageViewModel>();
            SimpleIoc.Default.Register<Settings.ApplicationsPageViewModel>();
            SimpleIoc.Default.Register<Settings.MainPageViewModel>();
            SimpleIoc.Default.Register<Settings.ToolsPageViewModel>();

            SimpleIoc.Default.Register<Tutorials.MainPageViewModel>();
            SimpleIoc.Default.Register<Tutorials.UsbPageViewModel>();

            SimpleIoc.Default.Register<Transfers.WatchfacePageViewModel>();
            SimpleIoc.Default.Register<Transfers.WallpapersPageViewModel>();

            SimpleIoc.Default.Register<Controls.ScpCredentialsDialogViewModel>();
            SimpleIoc.Default.Register<Controls.ScreenshotsBenchmarkDialogViewModel>();

            SimpleIoc.Default.Register(InitializeNavigationService);
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<Services.ApplicationsService>(createInstanceImmediately: true);

            if (!Common.Helpers.TilesHelper.BatteryTileExists())
            {
                BackgroundManager.Unregister(BackgroundManager.TimeBatteryLevelTaskName);
            }
        }

        private INavigationService InitializeNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure(nameof(HomeWelcome), typeof(Views.Home.WelcomePage));
            navigationService.Configure(nameof(Home), typeof(Views.Home.MainPage));
            navigationService.Configure(nameof(HomeScreenshots), typeof(Views.Home.ScreenshotsPage));

            navigationService.Configure(nameof(SettingsAbout), typeof(Views.Settings.AboutPage));
            navigationService.Configure(nameof(Settings), typeof(Views.Settings.MainPage));
            navigationService.Configure(nameof(SettingsApplications), typeof(Views.Settings.ApplicationsPage));
            navigationService.Configure(nameof(SettingsApplication), typeof(Views.Settings.ApplicationPage));
            navigationService.Configure(nameof(SettingsTools), typeof(Views.Settings.ToolsPage));

            navigationService.Configure(nameof(TransfersWatchface), typeof(Views.Transfers.WatchfacePage));
            navigationService.Configure(nameof(TransfersWallpapers), typeof(Views.Transfers.WallpapersPage));

            navigationService.Configure(nameof(Tutorials), typeof(Views.Tutorials.MainPage));
            navigationService.Configure(nameof(TutorialsUsb), typeof(Views.Tutorials.UsbPage));

            return navigationService;
        }

        public static BasePageViewModel GetCurrentViewModel(string pageKey)
        {
            switch (pageKey)
            {
                case nameof(Home):
                    return Home;
                case nameof(HomeScreenshots):
                    return HomeScreenshots;
                case nameof(HomeWelcome):
                    return HomeWelcome;
                case nameof(Settings):
                    return Settings;
                case nameof(SettingsAbout):
                    return SettingsAbout;
                case nameof(SettingsApplication):
                    return SettingsApplication;
                case nameof(SettingsApplications):
                    return SettingsApplications;
                case nameof(SettingsTools):
                    return SettingsTools;
                case nameof(TransfersWallpapers):
                    return TransfersWallpapers;
                case nameof(TransfersWatchface):
                    return TransfersWatchface;
                case nameof(Tutorials):
                    return Tutorials;
                case nameof(TutorialsUsb):
                    return TutorialsUsb;
                default:
                    throw new ArgumentException(nameof(pageKey));
            }
        }

        public static Home.MainPageViewModel Home
        {
            get { return ServiceLocator.Current.GetInstance<Home.MainPageViewModel>(); }
        }

        public static Home.ScreenshotsPageViewModel HomeScreenshots
        {
            get { return ServiceLocator.Current.GetInstance<Home.ScreenshotsPageViewModel>(); }
        }

        public static Home.WelcomePageViewModel HomeWelcome
        {
            get { return ServiceLocator.Current.GetInstance<Home.WelcomePageViewModel>(); }
        }

        public static Settings.AboutPageViewModel SettingsAbout
        {
            get { return ServiceLocator.Current.GetInstance<Settings.AboutPageViewModel>(); }
        }

        public static Settings.ApplicationPageViewModel SettingsApplication
        {
            get { return ServiceLocator.Current.GetInstance<Settings.ApplicationPageViewModel>(); }
        }

        public static Settings.ApplicationsPageViewModel SettingsApplications
        {
            get { return ServiceLocator.Current.GetInstance<Settings.ApplicationsPageViewModel>(); }
        }

        public static Settings.MainPageViewModel Settings
        {
            get { return ServiceLocator.Current.GetInstance<Settings.MainPageViewModel>(); }
        }

        public static Settings.ToolsPageViewModel SettingsTools
        {
            get { return ServiceLocator.Current.GetInstance<Settings.ToolsPageViewModel>(); }
        }

        public static Transfers.WatchfacePageViewModel TransfersWatchface
        {
            get { return ServiceLocator.Current.GetInstance<Transfers.WatchfacePageViewModel>(); }
        }

        public static Transfers.WallpapersPageViewModel TransfersWallpapers
        {
            get { return ServiceLocator.Current.GetInstance<Transfers.WallpapersPageViewModel>(); }
        }

        public static Tutorials.MainPageViewModel Tutorials
        {
            get { return ServiceLocator.Current.GetInstance<Tutorials.MainPageViewModel>(); }
        }

        public static Tutorials.UsbPageViewModel TutorialsUsb
        {
            get { return ServiceLocator.Current.GetInstance<Tutorials.UsbPageViewModel>(); }
        }

        public static Controls.ScpCredentialsDialogViewModel ScpCredentials
        {
            get { return ServiceLocator.Current.GetInstance<Controls.ScpCredentialsDialogViewModel>(); }
        }

        public static Controls.ScreenshotsBenchmarkDialogViewModel ScreenshotsBenchmark
        {
            get { return ServiceLocator.Current.GetInstance<Controls.ScreenshotsBenchmarkDialogViewModel>(); }
        }

        public static void Clear<T>() where T : ViewModelBase
        {
            SimpleIoc.Default.Unregister<T>();
            SimpleIoc.Default.Register<T>();
        }
    }
}
