using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class IconsPageViewModel : ViewModelBase
    {
        private readonly INavigationService NavigationService;

        public IconsPageViewModel(INavigationService navigationService)
        {
            this.NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.IconPreferences = new ObservableCollection<ApplicationPreferenceViewModel>();
            
            this.RefreshIconsPreferences();
        }

        private ObservableCollection<ApplicationPreferenceViewModel> _iconPreferences;
        public ObservableCollection<ApplicationPreferenceViewModel> IconPreferences
        {
            get { return _iconPreferences; }
            set { Set(nameof(IconPreferences), ref _iconPreferences, value); }
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

        private RelayCommand _backCommand;
        public RelayCommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new RelayCommand(GoBack);
                }

                return _backCommand;
            }
        }

        private void GoBack()
        {
            this.NavigationService.GoBack();
        }

        private void RefreshIconsPreferences()
        {
            this.IconPreferences.Clear();

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
