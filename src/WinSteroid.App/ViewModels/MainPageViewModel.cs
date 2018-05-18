using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using WinSteroid.App.Models;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;

        public MainPageViewModel(DeviceService deviceService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            this.UpdateBatteryStatus();
        }

        private ushort _batteryPercentage;
        public ushort BatteryPercentage
        {
            get { return _batteryPercentage; }
            set { Set(nameof(BatteryPercentage), ref _batteryPercentage, value); }
        }

        private BatteryLevel _batteryLevel;
        public BatteryLevel BatteryLevel
        {
            get { return _batteryLevel; }
            set { Set(nameof(BatteryLevel), ref _batteryLevel, value); }
        }

        private RelayCommand _updateBatteryStatusCommand;
        public RelayCommand UpdateBatteryStatusCommand
        {
            get
            {
                if (_updateBatteryStatusCommand == null)
                {
                    _updateBatteryStatusCommand = new RelayCommand(UpdateBatteryStatus);
                }

                return _updateBatteryStatusCommand;
            }
        }

        private async void UpdateBatteryStatus()
        {
            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();

            this.BatteryPercentage = batteryPercentage;
            this.BatteryLevel = BatteryLevelExtensions.Parse(batteryPercentage);
        }
    }
}
