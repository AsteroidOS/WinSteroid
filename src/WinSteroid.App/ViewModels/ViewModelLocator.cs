using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

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
            SimpleIoc.Default.Register<Data.Database>(createInstanceImmediately: true);
            SimpleIoc.Default.Register<Services.AsteroidService>(createInstanceImmediately: true);
            SimpleIoc.Default.Register<Services.DeviceService>(createInstanceImmediately: true);
            SimpleIoc.Default.Register<Services.BackgroundService>();
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
