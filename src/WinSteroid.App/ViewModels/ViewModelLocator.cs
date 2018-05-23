﻿using CommonServiceLocator;
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
            SimpleIoc.Default.Register<IconsPageViewModel>();
            SimpleIoc.Default.Register<ApplicationPageViewModel>();

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
            navigationService.Configure(nameof(Icons), typeof(Views.IconsPage));
            navigationService.Configure(nameof(Application), typeof(Views.ApplicationPage));

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
                case nameof(Icons):
                    return Icons;
                case nameof(Application):
                    return Application;
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

        public static IconsPageViewModel Icons
        {
            get { return ServiceLocator.Current.GetInstance<IconsPageViewModel>(); }
        }

        public static ApplicationPageViewModel Application
        {
            get { return ServiceLocator.Current.GetInstance<ApplicationPageViewModel>(); }
        }
    }
}
