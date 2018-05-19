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
    }
}
