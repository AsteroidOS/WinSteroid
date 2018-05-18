using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using Windows.ApplicationModel.Background;
using WinSteroid.App.Models;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly DeviceService DeviceService;
        private readonly BackgroundService BackgroundService;
        private readonly NotificationsService NotificationsService;
        private readonly IDialogService DialogService;
        private readonly BackgroundTaskProgressEventHandler ProgressEventHandler;

        public MainPageViewModel(DeviceService deviceService, BackgroundService backgroundService, NotificationsService notificationsService, IDialogService dialogService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            this.BackgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
            this.NotificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.ProgressEventHandler = new BackgroundTaskProgressEventHandler(OnProgress);
            
            this.InitializeBatteryLevelHandlers();
            this.InitializeNotificationsHandlers();
            this.InitializeActiveNotificationHandlers();
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

        private async void OnProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            await DispatcherHelper.RunAsync(UpdateBatteryStatus);
        }

        private async void InitializeBatteryLevelHandlers()
        {
            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(Asteroid.BatteryLevelCharacteristicUuid);

            this.BackgroundService.RegisterBatteryLevelTask(characteristic);

            var registrationCompleted = this.BackgroundService.TryToRegisterBatteryLevelBackgroundTaskProgressHandler(this.ProgressEventHandler);
            if (registrationCompleted)
            {
                this.UpdateBatteryStatus();
                return;
            }

            await this.DialogService.ShowMessage("I cannot be able to finish battery status handlers registration!", "Error");
        }

        private async void InitializeActiveNotificationHandlers()
        {
            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(Asteroid.NotificationFeedbackCharacteristicUuid);

            this.BackgroundService.RegisterActiveNotificationTask(characteristic);
        }

        private async void InitializeNotificationsHandlers()
        {
            this.BackgroundService.RegisterUserNotificationTask();

            await this.NotificationsService.RetriveNotificationsAsync();
        }

        private async void UpdateBatteryStatus()
        {
            var batteryPercentage = await this.DeviceService.GetBatteryPercentageAsync();

            this.BatteryPercentage = batteryPercentage;
            this.BatteryLevel = BatteryLevelExtensions.Parse(batteryPercentage);
        }
    }
}
