using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using Windows.ApplicationModel;

namespace WinSteroid.App.ViewModels.Settings
{
    public class AboutPageViewModel : BasePageViewModel
    {
        public AboutPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            this.ApplicationName = Package.Current.DisplayName;
            this.ApplicationVersion = Package.Current.Id.GetVersion();
        }

        public override void Reset()
        {

        }

        private string _applicationVersion;
        public string ApplicationVersion
        {
            get { return _applicationVersion; }
            set { Set(nameof(ApplicationVersion), ref _applicationVersion, value); }
        }

        private string _applicationName;
        public string ApplicationName
        {
            get { return _applicationName; }
            set { Set(nameof(ApplicationName), ref _applicationName, value); }
        }

        private List<SoftwareItem> _usedSoftwares;
        public List<SoftwareItem> UsedSoftwares
        {
            get { return _usedSoftwares; }
            set { Set(nameof(UsedSoftwares), ref _usedSoftwares, value); }
        }
    }

    public class SoftwareItem
    {
        public string Name { get; set; }

        public string Authors { get; set; }

        public string License { get; set; }

        public string Url { get; set; }
    }
}
