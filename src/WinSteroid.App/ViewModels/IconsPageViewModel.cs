using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class IconsPageViewModel : BasePageViewModel
    {
        private readonly ApplicationsService ApplicationsService;

        public IconsPageViewModel(
            ApplicationsService applicationsService,
            IDialogService dialogService,
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.ApplicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));

            this.Initialize();
        }

        public override void Initialize()
        {
            this.IconPreferences = new ObservableCollection<ApplicationPreferenceViewModel>();

            this.Initialized = true;
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        private ObservableCollection<ApplicationPreferenceViewModel> _iconPreferences;
        public ObservableCollection<ApplicationPreferenceViewModel> IconPreferences
        {
            get { return _iconPreferences; }
            set { Set(nameof(IconPreferences), ref _iconPreferences, value); }
        }

        private ApplicationPreferenceViewModel _selectedPreferences;
        public ApplicationPreferenceViewModel SelectedPreferences
        {
            get { return _selectedPreferences; }
            set
            {
                if (!Set(nameof(SelectedPreferences), ref _selectedPreferences, value)) return;

                if (_selectedPreferences == null) return;

                this.NavigationService.NavigateTo(nameof(ViewModelLocator.Application), _selectedPreferences.Id);

                this.SelectedPreferences = null;
            }
        }

        public void RefreshIconsPreferences()
        {
            if (this.IconPreferences == null)
            {
                this.IconPreferences = new ObservableCollection<ApplicationPreferenceViewModel>();
            }
            else
            {
                this.IconPreferences.Clear();
            }

            var iconPreferences = this.ApplicationsService.UserIcons
                .OrderBy(ui => ui.PackageName)
                .Select(ui => new ApplicationPreferenceViewModel
                {
                    Id = ui.AppId,
                    Name = ui.PackageName,
                    Icon = ui.Icon,
                    Muted = ui.Muted
                })
                .ToArray();

            foreach (var iconPreference in iconPreferences)
            {
                this.IconPreferences.Add(iconPreference);
            }
        }
    }
}
