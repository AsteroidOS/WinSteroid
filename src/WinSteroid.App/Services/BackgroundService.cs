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
        public const string BatteryLevelTaskName = nameof(BatteryLevelBackgroundTask);
        private static readonly string BatteryLevelTaskEntryPoint = typeof(BatteryLevelBackgroundTask).FullName;

        public const string ActiveNotificationTaskName = nameof(ActiveNotificationBackgroundTask);
        private static readonly string ActiveNotificationTaskEntryPoint = typeof(ActiveNotificationBackgroundTask).FullName;

        public const string SystemSessionConnectedTaskName = nameof(SystemSessionConnectedTask);
        private static readonly string SystemSessionConnectedTaskEntryPoint = typeof(SystemSessionConnectedTask).FullName;

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

        public bool RegisterBatteryLevelBackgroundTaskEventHandler(BackgroundTaskProgressEventHandler progressEventHandler)
        {
            if (progressEventHandler == null)
            {
                throw new ArgumentNullException(nameof(progressEventHandler));
            }

            var backgroundTask = this.GetBackgroundTask(BatteryLevelTaskName);
            if (backgroundTask == null) return false;

            backgroundTask.Progress += progressEventHandler;

            return true;
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

        //public async Task<bool> RegisterSystemSessionConnectedTask()
        //{
        //    if (IsBackgroundTaskRegistered(SystemSessionConnectedTaskName)) return true;

        //    var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
        //    if (!canExecuteBackgroundTasks) return false;

        //    var builder = new BackgroundTaskBuilder
        //    {
        //        Name = SystemSessionConnectedTaskName,
        //        TaskEntryPoint = SystemSessionConnectedTaskEntryPoint
        //    };
        //    builder.SetTrigger(new SystemTrigger(SystemTriggerType.SessionConnected, oneShot: false));
        //    var result = builder.Register();

        //    return result != null;
        //}

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

        private async Task<bool> CheckIfApplicationCanExecuteBackgroundTasks()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            return backgroundAccessStatus != BackgroundAccessStatus.DeniedByUser
                && backgroundAccessStatus != BackgroundAccessStatus.DeniedBySystemPolicy
                && backgroundAccessStatus != BackgroundAccessStatus.Unspecified;
        }

        public void UnregisterAllTasks()
        {
            Unregister(ActiveNotificationTaskName);
            Unregister(BatteryLevelTaskName);
            Unregister(SystemSessionConnectedTaskName);
            Unregister(UserNotificationsTaskName);
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

        private IBackgroundTaskRegistration GetBackgroundTask(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(task => StringExtensions.OrdinalIgnoreCaseEquals(task.Name, taskName));
        }
    }
}
