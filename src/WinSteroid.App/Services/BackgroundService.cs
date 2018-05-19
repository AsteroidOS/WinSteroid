using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Notifications;
using WinSteroid.Services;

namespace WinSteroid.App.Services
{
    public class BackgroundService
    {
        private const string BatteryLevelTaskName = nameof(BatteryLevelBackgroundTask);
        private const string BatteryLevelTaskEntryPoint = nameof(WinSteroid) + "." + nameof(Services) + "." + nameof(BatteryLevelBackgroundTask);

        private const string ActiveNotificationTaskName = nameof(BatteryLevelBackgroundTask);
        private const string ActiveNotificationTaskEntryPoint = nameof(WinSteroid) + "." + nameof(Services) + "." + nameof(BatteryLevelBackgroundTask);

        public const string UserNotificationsTaskName = "UserNotificationsTask";

        public async Task<bool> RegisterBatteryLevelTask(GattCharacteristic characteristic)
        {
            if (IsBackgroundTaskRegistered(BatteryLevelTaskName)) return true;

            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.DeniedByUser
                || backgroundAccessStatus == BackgroundAccessStatus.DeniedBySystemPolicy
                || backgroundAccessStatus == BackgroundAccessStatus.Unspecified) return false;

            var builder = new BackgroundTaskBuilder
            {
                Name = BatteryLevelTaskName,
                TaskEntryPoint = BatteryLevelTaskEntryPoint
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            builder.Register();

            return true;
        }

        //public async Task<bool> RegisterActiveNotificationTask(GattCharacteristic characteristic)
        //{
        //    if (IsBackgroundTaskRegistered(ActiveNotificationTaskName)) return true;

        //    var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
        //    if (backgroundAccessStatus == BackgroundAccessStatus.DeniedByUser
        //        || backgroundAccessStatus == BackgroundAccessStatus.DeniedBySystemPolicy
        //        || backgroundAccessStatus == BackgroundAccessStatus.Unspecified) return false;

        //    var builder = new BackgroundTaskBuilder
        //    {
        //        Name = ActiveNotificationTaskName,
        //        TaskEntryPoint = ActiveNotificationTaskEntryPoint
        //    };

        //    var trigger = new GattCharacteristicNotificationTrigger(characteristic);
        //    trigger.Characteristic.ValueChanged += OnActiveNotificationTriggerCharacteristicValueChanged;

        //    builder.SetTrigger(trigger);
        //    builder.Register();

        //    return true;
        //}

        public void RegisterUserNotificationTask()
        {
            if (IsBackgroundTaskRegistered(BatteryLevelTaskName)) return;

            var builder = new BackgroundTaskBuilder
            {
                Name = UserNotificationsTaskName
            };
            builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
            builder.Register();
        }

        public void Unregister(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (task.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase))
                {
                    task.Unregister(cancelTask: true);
                }
            }
        }

        private static bool IsBackgroundTaskRegistered(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Any(kvp => kvp.Value.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));
        }

        private static IBackgroundTaskRegistration GetBackgroundTask(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks.Values)
            {
                if (task.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase)) return task;
            }

            return null;
        }

        public bool TryToRegisterBatteryLevelBackgroundTaskProgressHandler(BackgroundTaskProgressEventHandler progressEventHandler)
        {
            return TryToRegisterBackgroundTaskEventHandler(BatteryLevelTaskName, null, progressEventHandler);
        }

        private bool TryToRegisterBackgroundTaskEventHandler(string taskName, BackgroundTaskCompletedEventHandler completedEventHandler, BackgroundTaskProgressEventHandler progressEventHandler)
        {
            var backgroundTask = GetBackgroundTask(taskName);
            if (backgroundTask == null) return false;

            if (completedEventHandler != null)
            {
                backgroundTask.Completed += completedEventHandler;
            }

            if (progressEventHandler != null)
            {
                backgroundTask.Progress += progressEventHandler;
            }

            return true;
        }
    }
}
