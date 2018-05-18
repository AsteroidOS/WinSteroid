using System;
using System.Linq;
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

        public void RegisterBatteryLevelTask(GattCharacteristic characteristic)
        {
            if (IsBackgroundTaskRegistered(BatteryLevelTaskName)) return;

            var builder = new BackgroundTaskBuilder
            {
                Name = BatteryLevelTaskName,
                TaskEntryPoint = BatteryLevelTaskEntryPoint
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            builder.Register();
        }

        public void RegisterActiveNotificationTask(GattCharacteristic characteristic)
        {
            if (IsBackgroundTaskRegistered(BatteryLevelTaskName)) return;

            var builder = new BackgroundTaskBuilder
            {
                Name = ActiveNotificationTaskName,
                TaskEntryPoint = ActiveNotificationTaskEntryPoint
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            builder.Register();
        }

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
