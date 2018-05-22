using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class IconsPageViewModel : BasePageViewModel
    {
        public IconsPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
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

        private RelayCommand _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(SavePreferences);
                }

                return _saveCommand;
            }
        }

        private async void SavePreferences()
        {
            await ApplicationsHelper.SaveUserIcons();
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

            var iconPreferences = ApplicationsHelper.UserIcons
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
