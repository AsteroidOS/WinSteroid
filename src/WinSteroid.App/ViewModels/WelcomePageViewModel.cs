using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;
        private readonly DispatcherTimer DispatcherTimer;

        public WelcomePageViewModel(DeviceService deviceService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.Devices = new ObservableCollection<Device>();
            this.DispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            this.DispatcherTimer.Tick += OnDispatcherTimerTick;
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(nameof(IsBusy), ref _isBusy, value); }
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

        private void StartSearch()
        {
            this.IsBusy = true;
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
            if (this.SelectedDevice == null) return;

            var paired = await this.DeviceService.ConnectAsync(this.SelectedDevice.Id);
            System.Diagnostics.Debug.WriteLine("Paired with " + this.SelectedDevice.Id);
        }

        private void OnDispatcherTimerTick(object sender, object e)
        {
            if (!this.DeviceService.IsSearching())
            {
                this.StopSearch();
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
