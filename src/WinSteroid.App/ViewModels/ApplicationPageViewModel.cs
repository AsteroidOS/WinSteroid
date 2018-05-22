using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class ApplicationPageViewModel : BasePageViewModel
    {
        public ApplicationPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.Initialize();
        }

        public override void Initialize()
        {
            this.Icons = ApplicationIconExtensions.GetList();

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

        private List<ApplicationIcon> _icons;
        public List<ApplicationIcon> Icons
        {
            get { return _icons; }
            set { Set(nameof(Icons), ref _icons, value); }
        }

        private ApplicationIcon _icon;
        public ApplicationIcon Icon
        {
            get { return _icon; }
            set { Set(nameof(Icon), ref _icon, value); }
        }

        private bool _muted;
        public bool Muted
        {
            get { return _muted; }
            set { Set(nameof(Muted), ref _muted, value); }
        }

        public List<VibrationLevel> AvailableVibrationLevels
        {
            get
            {
                return EnumExtensions.GetValues<VibrationLevel>()
                    .OrderBy(vl => (int)vl)
                    .ToList();
            }
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
            ApplicationsHelper.UpsertUserIcon(this.Id, this.Name, this.Icon, this.Muted);

            await ApplicationsHelper.SaveUserIcons();

            this.NavigationService.GoBack();
        }

        public void Load(string appId)
        {
            var application = ApplicationsHelper.GetApplicationPreferenceByAppId(appId);

            this.Id = application.AppId;
            this.Name = application.PackageName;
            this.Icon = application.Icon;
            this.Muted = application.Muted;
            this.Vibration = application.Vibration;
        }

        public bool CheckUnsavedChanges()
        {
            var application = ApplicationsHelper.GetApplicationPreferenceByAppId(this.Id);

            return this.Icon != application.Icon 
                || this.Muted != application.Muted
                || this.Vibration != application.Vibration;
        }

        public void Reset()
        {
            this.Id = string.Empty;
            this.Name = string.Empty;
            this.Icon = ApplicationIcon.Alert;
            this.Muted = false;
            this.Vibration = VibrationLevel.None;
        }
    }
}
