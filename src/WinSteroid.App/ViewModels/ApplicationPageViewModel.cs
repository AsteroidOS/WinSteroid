using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using WinSteroid.App.Services;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
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

        public override void Initialize()
        {
            this.AvailableIcons = ApplicationIconExtensions.GetList();

            var appId = SettingsHelper.GetValue("lastAppId", string.Empty);

            var application = this.ApplicationsService.GetApplicationPreferenceByAppId(appId);

            this.Id = application.AppId;
            this.Name = application.PackageName;
            this.SelectedIcon = this.AvailableIcons.FirstOrDefault(i => i == application.Icon);
            this.Muted = application.Muted;
            this.Vibration = application.Vibration;

            this.Initialized = true;
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

        public string Id { get; set; }

        public string Name { get; set; }

        private List<ApplicationIcon> _availableIcons;
        public List<ApplicationIcon> AvailableIcons
        {
            get { return _availableIcons; }
            set { Set(nameof(AvailableIcons), ref _availableIcons, value); }
        }

        private ApplicationIcon _selectedIcon;
        public ApplicationIcon SelectedIcon
        {
            get { return _selectedIcon; }
            set { Set(nameof(SelectedIcon), ref _selectedIcon, value); }
        }

        private bool _muted;
        public bool Muted
        {
            get { return _muted; }
            set { Set(nameof(Muted), ref _muted, value); }
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
