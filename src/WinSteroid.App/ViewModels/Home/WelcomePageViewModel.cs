﻿//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
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
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using WinSteroid.Common;
using WinSteroid.Common.Bluetooth;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels.Home
{
    public class WelcomePageViewModel : BasePageViewModel
    {
        public WelcomePageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            this.ApplicationFooter = $"{Package.Current.DisplayName} - {Package.Current.Id.GetVersion()}";

            var deviceId = SettingsHelper.GetValue(Constants.LastSavedDeviceIdSettingKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                this.Pair(deviceId);
                return;
            }

            this.ShowConnectionOptions = true;
        }

        public override void Reset()
        {
            this.ShowConnectionOptions = true;
        }

        private string _applicationFooter;
        public string ApplicationFooter
        {
            get { return _applicationFooter; }
            set { Set(nameof(ApplicationFooter), ref _applicationFooter, value); }
        }

        private bool _connectionFailed;
        public bool ConnectionFailed
        {
            get { return _connectionFailed; }
            set { Set(nameof(ConnectionFailed), ref _connectionFailed, value); }
        }

        private bool _showConnectionOptions;
        public bool ShowConnectionOptions
        {
            get { return _showConnectionOptions; }
            set { Set(nameof(ShowConnectionOptions), ref _showConnectionOptions, value); }
        }

        private RelayCommand _startSearchCommand;
        public RelayCommand StartSearchCommand
        {
            get
            {
                if (_startSearchCommand == null)
                {
                    _startSearchCommand = new RelayCommand(StartSearch);
                }

                return _startSearchCommand;
            }
        }

        private async void StartSearch()
        {
            this.ConnectionFailed = false;
            this.ShowConnectionOptions = false;

            var deviceId = SettingsHelper.GetValue(Constants.LastSavedDeviceIdSettingKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                this.Pair(deviceId);
                return;
            }
            
            var device = await DeviceManager.PickSingleDeviceAsync();
            if (device == null)
            {
                this.ShowConnectionOptions = true;
                return;
            }
            
            this.Pair(device.Id);
        }

        private RelayCommand _tutorialsCommand;
        public RelayCommand TutorialsCommand
        {
            get
            {
                if (_tutorialsCommand == null)
                {
                    _tutorialsCommand = new RelayCommand(GoToTutorials);
                }

                return _tutorialsCommand;
            }
        }

        private void GoToTutorials()
        {
            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Tutorials));
        }

        private async void Pair(string deviceId)
        {
            this.IsBusy = true;
            this.BusyMessage = ResourcesHelper.GetLocalizedString("HomeWelcomePairingMessage");
            this.ConnectionFailed = false;

            var errorMessage = await DeviceManager.ConnectAsync(deviceId, isBackgroundActivity: false);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                this.ConnectionFailed = true;
                await this.DialogService.ShowMessage(errorMessage, ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
                return;
            }

            var pairingResult = await DeviceManager.PairAsync();

            this.IsBusy = false;
            this.BusyMessage = string.Empty;

            if (!pairingResult.IsSuccess)
            {
                this.ConnectionFailed = true;

                if (pairingResult.NeedSystemPairing)
                {
                    var openSettings = await this.DialogService.ShowConfirmMessage(
                        ResourcesHelper.GetLocalizedString("DeviceServiceSystemPairingRequiredMessage"),
                        ResourcesHelper.GetLocalizedString("DeviceServiceSystemPairingRequiredTitle"));

                    if (openSettings)
                    {
                        await Launcher.LaunchUriAsync(new Uri("ms-settings:bluetooth"));
                        return;
                    }
                }
                
                await this.DialogService.ShowMessage(
                    ResourcesHelper.GetLocalizedString("HomeWelcomePairingOperationFailedError"), 
                    ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
                return;
            }

            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Home));
        }
    }
}
