using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class ApplicationsPageViewModel : BasePageViewModel
    {
        private readonly ApplicationsService ApplicationsService;

        public ApplicationsPageViewModel(
            ApplicationsService applicationsService,
            IDialogService dialogService,
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.ApplicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));

            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            this.IconPreferences = new ObservableCollection<ApplicationViewModel>();

            this.Initialized = true;
        }

        public override void Reset()
        {
            
        }

        private ObservableCollection<ApplicationViewModel> _iconPreferences;
        public ObservableCollection<ApplicationViewModel> IconPreferences
        {
            get { return _iconPreferences; }
            set { Set(nameof(IconPreferences), ref _iconPreferences, value); }
        }

        private ApplicationViewModel _selectedPreferences;
        public ApplicationViewModel SelectedPreferences
        {
            get { return _selectedPreferences; }
            set
            {
                if (!Set(nameof(SelectedPreferences), ref _selectedPreferences, value)) return;

                if (_selectedPreferences == null) return;

                SettingsHelper.SetValue("lastAppId", _selectedPreferences.Id);

                this.NavigationService.NavigateTo(nameof(ViewModelLocator.Application));

                this.SelectedPreferences = null;
            }
        }

        public void RefreshIconsPreferences()
        {
            if (this.IconPreferences == null)
            {
                this.IconPreferences = new ObservableCollection<ApplicationViewModel>();
            }
            else
            {
                this.IconPreferences.Clear();
            }

            var iconPreferences = this.ApplicationsService.UserIcons
                .OrderBy(ui => ui.PackageName)
                .Select(ToApplicationViewModel)
                .ToArray();

            foreach (var iconPreference in iconPreferences)
            {
                this.IconPreferences.Add(iconPreference);
            }
        }

        private ApplicationViewModel ToApplicationViewModel(ApplicationPreference applicationPreference)
        {
            return new ApplicationViewModel
            {
                Id = applicationPreference.AppId,
                Name = applicationPreference.PackageName,
                Icon = applicationPreference.Icon,
                Muted = applicationPreference.Muted,
                Vibration = applicationPreference.Muted ? false : applicationPreference.Vibration != VibrationLevel.None
            };
        }
    }

    public class ApplicationViewModel
    {
        public ApplicationIcon Icon { get; set; }

        public string Id { get; set; }

        public bool Muted { get; set; }

        public string Name { get; set; }

        public bool Vibration { get; set; }
    }
}
