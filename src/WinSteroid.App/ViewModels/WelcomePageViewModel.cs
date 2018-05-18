using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using WinSteroid.App.Helpers;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;
        private readonly INavigationService NavigationService;
        private readonly IDialogService DialogService;
        private readonly DispatcherTimer DispatcherTimer;

        public WelcomePageViewModel(DeviceService deviceService, INavigationService navigationService, IDialogService dialogService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.Devices = new ObservableCollection<Device>();
            this.DispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            this.DispatcherTimer.Tick += OnDispatcherTimerTick;

            this.DiscoverPairedDevice();
        }

        private const ushort MaxTicksCount = 6;
        private ushort TickCount = 0;
        private string LastSavedDeviceId;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(nameof(IsBusy), ref _isBusy, value); }
        }

        private string _busyMessage;
        public string BusyMessage
        {
            get { return _busyMessage; }
            set { Set(nameof(BusyMessage), ref _busyMessage, value); }
        }

        private bool _isPairedSearch;
        public bool IsPairedSearch
        {
            get { return _isPairedSearch; }
            set { Set(nameof(IsPairedSearch), ref _isPairedSearch, value); }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set { Set(nameof(IsSearching), ref _isSearching, value); }
        }

        private bool _pairButtonEnabled;
        public bool PairButtonEnabled
        {
            get { return _pairButtonEnabled; }
            set { Set(nameof(PairButtonEnabled), ref _pairButtonEnabled, value); }
        }

        private ObservableCollection<Device> _devices;
        public ObservableCollection<Device> Devices
        {
            get { return _devices; }
            set { Set(nameof(Devices), ref _devices, value); }
        }

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (!Set(nameof(SelectedDevice), ref _selectedDevice, value)) return;

                this.PairButtonEnabled = _selectedDevice != null && this.Devices.Any(d => d.Id == _selectedDevice.Id);
                this.RaisePropertyChanged(nameof(PairButtonEnabled));
            }
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

        private void DiscoverPairedDevice()
        {
            this.LastSavedDeviceId = SettingsHelper.GetValue("lastSavedDeviceId", string.Empty);
            if (string.IsNullOrWhiteSpace(this.LastSavedDeviceId)) return;

            this.IsPairedSearch = true;
            this.StartSearch();
        }

        private void StartSearch()
        {
            this.IsBusy = true;
            this.BusyMessage = "Searching";
            this.IsSearching = true;
            this.DeviceService.StartSearch();
            this.DispatcherTimer.Start();
        }

        private RelayCommand _stopSearchCommand;
        public RelayCommand StopSearchCommand
        {
            get
            {
                if (_stopSearchCommand == null)
                {
                    _stopSearchCommand = new RelayCommand(StopSearch);
                }

                return _stopSearchCommand;
            }
        }

        private void StopSearch()
        {
            this.DeviceService.StopSearch();
            this.DispatcherTimer.Stop();
            this.IsBusy = false;
            this.BusyMessage = string.Empty;
            this.IsSearching = false;
        }

        private RelayCommand _pairCommand;
        public RelayCommand PairCommand
        {
            get
            {
                if (_pairCommand == null)
                {
                    _pairCommand = new RelayCommand(Pair);
                }

                return _pairCommand;
            }
        }

        private async void Pair()
        {
            this.IsBusy = true;
            this.BusyMessage = "Pairing";

            if (string.IsNullOrWhiteSpace(this.LastSavedDeviceId) && this.SelectedDevice != null)
            {
                this.LastSavedDeviceId = this.SelectedDevice.Id;
            }

            var connected = await this.DeviceService.ConnectAsync(this.LastSavedDeviceId);
            if (!connected)
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                await this.DialogService.ShowMessage("I cannot connect to selected Bluetooth device", "Error");
                return;
            }

            var pairingResult = await this.DeviceService.PairAsync(this.LastSavedDeviceId);
            if (!pairingResult.IsSuccess)
            {
                this.IsBusy = false;
                this.BusyMessage = string.Empty;
                await this.DialogService.ShowMessage("Paired operation failed", "Error");
                return;
            }

            this.NavigationService.NavigateTo(nameof(ViewModelLocator.Main));
        }

        private void OnDispatcherTimerTick(object sender, object e)
        {
            if (this.IsPairedSearch && this.DeviceService.Devices.Any(d => d.Id == LastSavedDeviceId))
            {
                this.StopSearch();
                this.TickCount = 0;
                this.IsPairedSearch = false;
                this.Pair();
                return;
            }

            if (!this.DeviceService.IsSearching() || TickCount == MaxTicksCount)
            {
                this.StopSearch();
                this.TickCount = 0;
            }
            else
            {
                this.TickCount++;
            }

            if (this.DeviceService.Devices.Count == 0) return;

            foreach (var device in this.DeviceService.Devices)
            {
                if (this.Devices.Any(d => d.Id == device.Id)) continue;

                this.Devices.Add(new Device
                {
                    Id = device.Id,
                    Name = device.Name
                });
            }
        }

        public class Device
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}
