using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Notifications;
using WinSteroid.Common;

namespace WinSteroid.App.Services
{
    public class BackgroundService
    {
        private readonly DeviceService DeviceService;

        public BackgroundService(DeviceService deviceService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            this.RegisterSystemSessionTask();
        }

        public const string BatteryLevelTaskName = "BatteryLevelBackgroundTask";
        public const string ActiveNotificationTaskName = "ActiveNotificationBackgroundTask";
        public const string SystemSessionTaskName = "SystemSessionTask";
        public const string UserNotificationsTaskName = "UserNotificationsTask";

        private async Task<bool> CheckIfApplicationCanExecuteBackgroundTasks()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            return backgroundAccessStatus != BackgroundAccessStatus.DeniedByUser
                && backgroundAccessStatus != BackgroundAccessStatus.DeniedBySystemPolicy
                && backgroundAccessStatus != BackgroundAccessStatus.Unspecified;
        }

        private IBackgroundTaskRegistration GetBackgroundTask(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(task => StringExtensions.OrdinalIgnoreCaseEquals(task.Name, taskName));
        }

        public bool IsBackgroundTaskRegistered(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Any(kvp => StringExtensions.OrdinalIgnoreCaseEquals(kvp.Value.Name, taskName));
        }

        public async Task<bool> RegisterActiveNotificationTask()
        {
            if (IsBackgroundTaskRegistered(ActiveNotificationTaskName)) return true;

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;

            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(Asteroid.NotificationFeedbackCharacteristicUuid);

            var builder = new BackgroundTaskBuilder
            {
                Name = ActiveNotificationTaskName
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            var result = builder.Register();

            return result != null;
        }

        public async Task<bool> RegisterBatteryLevelTask()
        {
            if (IsBackgroundTaskRegistered(BatteryLevelTaskName)) return true;

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;

            var characteristic = await this.DeviceService.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);

            var builder = new BackgroundTaskBuilder
            {
                Name = BatteryLevelTaskName
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            var result = builder.Register();

            return result != null;
        }

        private bool RegisterSystemSessionTask()
        {
            if (IsBackgroundTaskRegistered(SystemSessionTaskName)) return true;

            var builder = new BackgroundTaskBuilder
            {
                Name = SystemSessionTaskName
            };
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.SessionConnected, oneShot: false));
            var result = builder.Register();

            return result != null;
        }

        public bool RegisterUserNotificationTask()
        {
            if (IsBackgroundTaskRegistered(UserNotificationsTaskName)) return true;

            var builder = new BackgroundTaskBuilder
            {
                Name = UserNotificationsTaskName
            };
            builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
            var result = builder.Register();

            return result != null;
        }

        public void Unregister(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (StringExtensions.OrdinalIgnoreCaseEquals(task.Name, taskName))
                {
                    task.Unregister(cancelTask: true);
                }
            }
        }
    }
}
