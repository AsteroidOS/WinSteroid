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

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;

namespace WinSteroid.App.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<WelcomePageViewModel>();
            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();
            SimpleIoc.Default.Register<ApplicationsPageViewModel>();
            SimpleIoc.Default.Register<ApplicationPageViewModel>();
            SimpleIoc.Default.Register<TutorialsPageViewModel>();

            SimpleIoc.Default.Register<Tutorial.UsbPageViewModel>();

            if (!Common.Helpers.ApiHelper.CheckIfIsSystemMobile())
            {
                SimpleIoc.Default.Register<WatchFacePageViewModel>();
                SimpleIoc.Default.Register<WallpapersPageViewModel>();
            }

            SimpleIoc.Default.Register(InitializeNavigationService);
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<Services.ApplicationsService>(createInstanceImmediately: true);
            SimpleIoc.Default.Register<Services.DeviceService>(createInstanceImmediately: true);
            SimpleIoc.Default.Register<Services.BackgroundService>();
            SimpleIoc.Default.Register<Services.NotificationsService>();

            var backgroundService = SimpleIoc.Default.GetInstance<Services.BackgroundService>();
            if (!Common.Helpers.TilesHelper.BatteryTileExists())
            {
                backgroundService.Unregister(Services.BackgroundService.BatteryLevelTaskName);
            }
        }

        private INavigationService InitializeNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure(nameof(Welcome), typeof(Views.WelcomePage));
            navigationService.Configure(nameof(Main), typeof(Views.MainPage));
            navigationService.Configure(nameof(Settings), typeof(Views.SettingsPage));
            navigationService.Configure(nameof(Applications), typeof(Views.ApplicationsPage));
            navigationService.Configure(nameof(Application), typeof(Views.ApplicationPage));
            navigationService.Configure(nameof(Tutorials), typeof(Views.TutorialsPage));

            navigationService.Configure(nameof(TutorialUsb), typeof(Views.Tutorials.UsbPage));

            if (!Common.Helpers.ApiHelper.CheckIfIsSystemMobile())
            {
                navigationService.Configure(nameof(WatchFace), typeof(Views.WatchFacePage));
                navigationService.Configure(nameof(Wallpapers), typeof(Views.WallpapersPage));
            }

            return navigationService;
        }

        public static BasePageViewModel GetCurrentViewModel(string pageKey)
        {
            switch (pageKey)
            {
                case nameof(Welcome):
                    return Welcome;
                case nameof(Main):
                    return Main;
                case nameof(Settings):
                    return Settings;
                case nameof(Applications):
                    return Applications;
                case nameof(Application):
                    return Application;
                case nameof(WatchFace):
                    return WatchFace;
                case nameof(Wallpapers):
                    return Wallpapers;
                case nameof(Tutorials):
                    return Tutorials;
                case nameof(TutorialUsb):
                    return TutorialUsb;
                default:
                    throw new ArgumentException(nameof(pageKey));
            }
        }

        public static WelcomePageViewModel Welcome
        {
            get { return ServiceLocator.Current.GetInstance<WelcomePageViewModel>(); }
        }

        public static MainPageViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainPageViewModel>(); }
        }

        public static SettingsPageViewModel Settings
        {
            get { return ServiceLocator.Current.GetInstance<SettingsPageViewModel>(); }
        }

        public static ApplicationsPageViewModel Applications
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationsPageViewModel>(); }
        }

        public static ApplicationPageViewModel Application
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationPageViewModel>(); }
        }

        public static WatchFacePageViewModel WatchFace
        {
            get
            {
                if (Common.Helpers.ApiHelper.CheckIfIsSystemMobile())
                {
                    throw new PlatformNotSupportedException();
                }

                return ServiceLocator.Current.GetInstance<WatchFacePageViewModel>();
            }
        }

        public static WallpapersPageViewModel Wallpapers
        {
            get
            {
                if (Common.Helpers.ApiHelper.CheckIfIsSystemMobile())
                {
                    throw new PlatformNotSupportedException();
                }

                return ServiceLocator.Current.GetInstance<WallpapersPageViewModel>();
            }
        }

        public static TutorialsPageViewModel Tutorials
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TutorialsPageViewModel>();
            }
        }

        public static Tutorial.UsbPageViewModel TutorialUsb
        {
            get
            {
                return ServiceLocator.Current.GetInstance<Tutorial.UsbPageViewModel>();
            }
        }

        public static void Clear<T>() where T : ViewModelBase
        {
            SimpleIoc.Default.Unregister<T>();
            SimpleIoc.Default.Register<T>();
        }
    }
}
