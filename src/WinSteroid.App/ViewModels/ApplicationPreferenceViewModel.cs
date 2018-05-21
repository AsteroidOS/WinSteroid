using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using Windows.UI.Xaml.Controls;
using WinSteroid.App.Controls;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class ApplicationPreferenceViewModel : ViewModelBase
    {
        public string Id { get; set; }

        public string Name { get; set; }

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
            set
            {
                if (!Set(nameof(Muted), ref _muted, value)) return;

                ApplicationsHelper.UpsertUserIcon(this.Id, this.Name, this.Icon, this.Muted);

                ApplicationsHelper.SaveUserIcons();
            }
        }

        private RelayCommand _changeIconCommand;
        public RelayCommand ChangeIconCommand
        {
            get
            {
                if (_changeIconCommand == null)
                {
                    _changeIconCommand = new RelayCommand(ChangeIcon);
                }

                return _changeIconCommand;
            }
        }

        private async void ChangeIcon()
        {
            var customIconDialog = new CustomIconDialog();
            var result = await customIconDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            this.Icon = customIconDialog.SelectedIcon.Icon;

            ApplicationsHelper.UpsertUserIcon(this.Id, this.Name, this.Icon, this.Muted);

            await ApplicationsHelper.SaveUserIcons();
        }
    }
}
