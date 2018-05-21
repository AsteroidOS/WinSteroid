using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Notifications;
using WinSteroidTasks;

namespace WinSteroid.App.Services
{
    public class BackgroundService
    {
        private const string BatteryLevelTaskName = nameof(BatteryLevelBackgroundTask);
        private static readonly string BatteryLevelTaskEntryPoint = typeof(BatteryLevelBackgroundTask).FullName;

        private const string ActiveNotificationTaskName = nameof(ActiveNotificationBackgroundTask);
        private static readonly string ActiveNotificationTaskEntryPoint = typeof(ActiveNotificationBackgroundTask).FullName;

        public const string UserNotificationsTaskName = "UserNotificationsTask";

        public async Task<bool> RegisterBatteryLevelTask(GattCharacteristic characteristic)
        {
            if (IsBackgroundTaskRegistered(BatteryLevelTaskName)) return true;

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;

            var builder = new BackgroundTaskBuilder
            {
                Name = BatteryLevelTaskName,
                TaskEntryPoint = BatteryLevelTaskEntryPoint
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            var result = builder.Register();

            return result != null;
        }

        public async Task<bool> RegisterActiveNotificationTask(GattCharacteristic characteristic)
        {
            if (IsBackgroundTaskRegistered(ActiveNotificationTaskName)) return true;

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;

            var builder = new BackgroundTaskBuilder
            {
                Name = ActiveNotificationTaskName,
                TaskEntryPoint = ActiveNotificationTaskEntryPoint
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            var result = builder.Register();

            return result != null;
        }

        public void RegisterUserNotificationTask()
        {
            if (IsBackgroundTaskRegistered(UserNotificationsTaskName)) return;

            var builder = new BackgroundTaskBuilder
            {
                Name = UserNotificationsTaskName
            };
            builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
            builder.Register();
        }

        private async Task<bool> CheckIfApplicationCanExecuteBackgroundTasks()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            return backgroundAccessStatus != BackgroundAccessStatus.DeniedByUser
                && backgroundAccessStatus != BackgroundAccessStatus.DeniedBySystemPolicy
                && backgroundAccessStatus != BackgroundAccessStatus.Unspecified;
        }

        public void UnregisterAllTasks()
        {
            Unregister(BatteryLevelTaskName);
            Unregister(ActiveNotificationTaskName);
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

        public bool IsBackgroundTaskRegistered(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Any(kvp => StringExtensions.OrdinalIgnoreCaseEquals(kvp.Value.Name, taskName));
        }

        private static IBackgroundTaskRegistration GetBackgroundTask(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (StringExtensions.OrdinalIgnoreCaseEquals(task.Name, taskName)) return task;
            }

            return null;
        }
    }
}
