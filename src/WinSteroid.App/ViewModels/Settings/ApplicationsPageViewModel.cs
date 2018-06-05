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

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinSteroid.App.Services;
using WinSteroid.Common;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels.Settings
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

                SettingsHelper.SetValue(Constants.LastAppIdSettingKey, _selectedPreferences.Id);

                this.NavigationService.NavigateTo(nameof(ViewModelLocator.SettingsApplication));

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

        private RelayCommand _exportCommand;
        public RelayCommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RelayCommand(Export);
                }

                return _exportCommand;
            }
        }

        private async void Export()
        {
            var result = await this.ApplicationsService.ExportDataAsync();
            if (result)
            {
                ToastsHelper.Show("Data successfully exported");
            }
        }

        private RelayCommand _importCommand;
        public RelayCommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new RelayCommand(Import);
                }

                return _importCommand;
            }
        }

        private async void Import()
        {
            try
            {
                var importedIconPreferences = await this.ApplicationsService.ImportDataAsync();
                if (importedIconPreferences.IsNullOrEmpty()) return;

                var overWriteExistingApplicationPreferences = false;
                if (importedIconPreferences.Any(ap => this.IconPreferences.Any(i => i.Id == ap.AppId)))
                {
                    overWriteExistingApplicationPreferences = await this.DialogService.ShowConfirmMessage(
                        "I found some conflicts. Do you want to overwrite existing application preferences when import them?", 
                        "Found conflicts");
                }

                foreach (var importedIconPreference in importedIconPreferences)
                {
                    var iconPreferenceExists = this.IconPreferences.Any(i => i.Id == importedIconPreference.AppId);
                    if (iconPreferenceExists && !overWriteExistingApplicationPreferences) continue;

                    this.ApplicationsService.UpsertUserIcon(importedIconPreference);
                }

                ToastsHelper.Show("Data successfully imported");

                this.RefreshIconsPreferences();
            }
            catch (Exception exception)
            {
                await this.DialogService.ShowError(exception.Message, "Error");
            }
        }

        private static ApplicationViewModel ToApplicationViewModel(ApplicationPreference applicationPreference)
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
