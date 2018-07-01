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
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Renci.SshNet;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinSteroid.App.Controls;
using WinSteroid.Common.Factory;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels.Settings
{
    public class ToolsPageViewModel : BasePageViewModel
    {
        private SshClient SshClient;

        public ToolsPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize() => this.Refresh();

        public override void Reset()
        {
            this.Initialized = false;
        }

        public bool Initialized { get; private set; }

        private List<UsbMode> _usbModes;
        public List<UsbMode> UsbModes
        {
            get { return _usbModes; }
            set { Set(nameof(UsbModes), ref _usbModes, value); }
        }

        private UsbMode _selectedUsbMode;
        public UsbMode SelectedUsbMode
        {
            get { return _selectedUsbMode; }
            set
            {
                if (value == null)
                {
                    value = new UsbMode { TypeMode = UsbModeEnum.None };
                }

                Set(nameof(SelectedUsbMode), ref _selectedUsbMode, value);
            }
        }

        private RelayCommand _applyUsbModeCommand;
        public RelayCommand ApplyUsbModeCommand
        {
            get
            {
                if (_applyUsbModeCommand == null)
                {
                    _applyUsbModeCommand = new RelayCommand(ApplyUsbMode);
                }

                return _applyUsbModeCommand;
            }
        }

        private async void ApplyUsbMode()
        {
            var success = this.SshClient.SetUsbMode(this.SelectedUsbMode.Key);
            if (!success)
            {
                await this.DialogService.ShowError(
                    ResourcesHelper.GetLocalizedString("SettingsToolsApplyUsbModeFailedError"),
                    ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
            }
        }

        private RelayCommand _rebootCommand;
        public RelayCommand RebootCommand
        {
            get
            {
                if (_rebootCommand == null)
                {
                    _rebootCommand = new RelayCommand(Reboot);
                }

                return _rebootCommand;
            }
        }

        private async void Reboot()
        {
            var sshRebootDialog = new SshRebootDialog();

            var result = await sshRebootDialog.ShowAsync();
            if (result == ContentDialogResult.Secondary || sshRebootDialog.SelectedRebootMode == SshRebootDialog.RebootMode.None) return;

            var commandExecuted = false;

            switch (sshRebootDialog.SelectedRebootMode)
            {
                case SshRebootDialog.RebootMode.Classic:
                    commandExecuted = this.SshClient.Reboot();
                    break;
                case SshRebootDialog.RebootMode.Bootloader:
                    commandExecuted = this.SshClient.RebootAndEnterBootloader();
                    break;
                case SshRebootDialog.RebootMode.Recovery:
                    commandExecuted = this.SshClient.RebootAndEnterRecovery();
                    break;
            }

            if (!commandExecuted) return;

            Application.Current.Exit();
        }

        public async void Refresh()
        {
            this.IsBusy = true;

            var scpCredentialsDialog = new ScpCredentialsDialog();
            var result = await scpCredentialsDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                this.IsBusy = false;
                this.NavigationService.GoBack();
                return;
            }

            try
            {
                this.SshClient = SecureConnectionsFactory.CreateSshClient(
                    scpCredentialsDialog.ViewModel.HostIP, 
                    scpCredentialsDialog.ViewModel.Username, 
                    scpCredentialsDialog.ViewModel.Password);
            }
            catch (Exception exception)
            {
                await this.DialogService.ShowError(exception, ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
            }

            if (this.SshClient == null)
            {
                this.IsBusy = false;
                this.NavigationService.GoBack();
                return;
            }

            this.UsbModes = this.SshClient.GetUsbModes();
            this.SelectedUsbMode = this.SshClient.GetCurrentUsbMode();

            this.IsBusy = false;
            this.Initialized = true;
        }
    }
}
