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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using WinSteroid.App.Services;
using WinSteroid.Common;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels.Settings
{
    public class ApplicationPageViewModel : BasePageViewModel
    {
        private readonly ApplicationsService ApplicationsService;

        public ApplicationPageViewModel(
            ApplicationsService applicationsService,
            IDialogService dialogService, 
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.ApplicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));

            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            if (!this.CheckUnsavedChanges()) return Task.FromResult(true);

            return this.DialogService.ShowMessage(
                message: "I detected some unsaved changes. Are you sure to discard them and go back?",
                title: "Unsaved changes",
                buttonConfirmText: "Yes",
                buttonCancelText: "No",
                afterHideCallback: r => { });
        }

        public override void Initialize()
        {
            this.AvailableIcons = ApplicationIconExtensions.GetList();

            var appId = SettingsHelper.GetValue(Constants.LastAppIdSettingKey, string.Empty);

            var application = this.ApplicationsService.GetApplicationPreferenceByAppId(appId);

            this.Id = application.AppId;
            this.Name = application.PackageName;
            this.SelectedIcon = this.AvailableIcons.FirstOrDefault(i => i == application.Icon);
            this.Muted = application.Muted;
            this.Vibration = application.Vibration;
        }

        public override void Reset()
        {
            
        }

        public string Id { get; set; }

        public string Name { get; set; }

        private List<ApplicationIcon> _availableIcons;
        public List<ApplicationIcon> AvailableIcons
        {
            get { return _availableIcons; }
            set { Set(nameof(AvailableIcons), ref _availableIcons, value); }
        }

        private bool _muted;
        public bool Muted
        {
            get { return _muted; }
            set { Set(nameof(Muted), ref _muted, value); }
        }

        private ApplicationIcon _selectedIcon;
        public ApplicationIcon SelectedIcon
        {
            get { return _selectedIcon; }
            set { Set(nameof(SelectedIcon), ref _selectedIcon, value); }
        }

        private VibrationLevel _vibration;
        public VibrationLevel Vibration
        {
            get { return _vibration; }
            set { Set(nameof(Vibration), ref _vibration, value); }
        }

        private RelayCommand _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(Save);
                }

                return _saveCommand;
            }
        }

        private async void Save()
        {
            this.ApplicationsService.UpsertUserIcon(this.Id, this.Name, this.SelectedIcon, this.Muted, this.Vibration);

            await this.ApplicationsService.SaveUserIcons();

            this.NavigationService.GoBack();
        }

        private bool CheckUnsavedChanges()
        {
            var application = this.ApplicationsService.GetApplicationPreferenceByAppId(this.Id);

            return this.SelectedIcon != application.Icon 
                || this.Muted != application.Muted
                || this.Vibration != application.Vibration;
        }
    }
}
