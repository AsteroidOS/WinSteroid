using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;

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

            SimpleIoc.Default.Register<INavigationService>(InitializeNavigationService);
            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<Services.DeviceService>(createInstanceImmediately: true);
            SimpleIoc.Default.Register<Services.BackgroundService>();
            SimpleIoc.Default.Register<Services.NotificationsService>();
        }

        private INavigationService InitializeNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure(nameof(Welcome), typeof(Views.WelcomePage));
            navigationService.Configure(nameof(Main), typeof(Views.MainPage));
            navigationService.Configure(nameof(Settings), typeof(Views.SettingsPage));
            navigationService.Configure(nameof(Icons), typeof(Views.IconsPage));

            return navigationService;
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
    }
}
