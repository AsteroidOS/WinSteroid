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
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class WelcomePageViewModel : BasePageViewModel
    {
        private readonly DeviceService DeviceService;

        public WelcomePageViewModel(
            DeviceService deviceService,
            IDialogService dialogService,
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            this.ApplicationFooter = $"{Package.Current.DisplayName} - {Package.Current.Id.GetVersion()}";

            var deviceId = this.DeviceService.GetLastSavedDeviceId();
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                this.DeviceId = deviceId;
                this.Pair();
            }
            else
            {
                this.ShowConnectionOptions = true;
            }

            this.Initialized = true;
        }

        public override void Reset()
        {
            this.DeviceId = string.Empty;
            this.DeviceName = string.Empty;
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

        private string _deviceId;
        public string DeviceId
        {
            get { return _deviceId; }
            set { Set(nameof(DeviceId), ref _deviceId, value); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { Set(nameof(DeviceName), ref _deviceName, value); }
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
            if (!string.IsNullOrWhiteSpace(this.DeviceId))
            {
                this.Pair();
                return;
            }
            
            var result = await this.DeviceService.PickSingleDeviceAsync(Views.WelcomePage.Current);
            if (!result)
            {
                this.ShowConnectionOptions = true;
                return;
            }

            this.DeviceId = this.DeviceService.Current.Id;
            this.DeviceName = this.DeviceService.Current.Name;

            this.Pair();
        }
        
        private RelayCommand _pairCommand;
        public RelayCommand PairCommand
        {
            get
            {
                if (_pairCommand == null)
                {
                    _pairCommand = new RelayCommand(Pair, CanPair);
                }

                return _pairCommand;
            }
        }

        private bool CanPair()
        {
            return !string.IsNullOrWhiteSpace(this.DeviceId) || this.DeviceService.Current != null;
        }

        private async void Pair()
        {
            this.IsBusy = true;
            this.BusyMessage = "Pairing";
            this.ConnectionFailed = false;

            if (string.IsNullOrWhiteSpace(this.DeviceId) && this.DeviceService.Current != null)
            {
                this.DeviceId = this.DeviceService.Current.Id;
                this.DeviceName = this.DeviceService.Current.Name;
            }

            var connected = await this.DeviceService.ConnectAsync(this.DeviceId);
            if (!connected)
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                this.ConnectionFailed = true;
                await this.DialogService.ShowMessage("I cannot connect to selected Bluetooth device", "Error");
                return;
            }

            var pairingResult = await this.DeviceService.PairAsync();
            if (!pairingResult.IsSuccess)
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                this.ConnectionFailed = true;
                await this.DialogService.ShowMessage("Paired operation failed", "Error");
                return;
            }

            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Main));
        }
    }
}
