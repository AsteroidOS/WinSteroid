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

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Notifications;

namespace WinSteroid.Common.Background
{
    public static class BackgroundManager
    {
        public const string ActiveNotificationTaskName = "ActiveNotificationBackgroundTask";
        public const string TimeBatteryLevelTaskName = "TimeBatteryLevelBackgroundTask";
        public const string UserNotificationsTaskName = "UserNotificationsTask";

        private static async Task<bool> CheckIfApplicationCanExecuteBackgroundTasks()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            return backgroundAccessStatus != BackgroundAccessStatus.DeniedByUser
                && backgroundAccessStatus != BackgroundAccessStatus.DeniedBySystemPolicy
                && backgroundAccessStatus != BackgroundAccessStatus.Unspecified;
        }

        private static IBackgroundTaskRegistration GetBackgroundTask(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(task => StringExtensions.OrdinalIgnoreCaseEquals(task.Name, taskName));
        }

        public static bool IsBackgroundTaskRegistered(string taskName)
        {
            return BackgroundTaskRegistration.AllTasks.Any(kvp => StringExtensions.OrdinalIgnoreCaseEquals(kvp.Value.Name, taskName));
        }

        public static async Task<bool> RegisterActiveNotificationTask(GattCharacteristic characteristic)
        {
            if (characteristic == null) return false;

            if (IsBackgroundTaskRegistered(ActiveNotificationTaskName)) return true;

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;

            var builder = new BackgroundTaskBuilder
            {
                Name = ActiveNotificationTaskName
            };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            var result = builder.Register();

            return result != null;
        }

        public static async Task<bool> RegisterTimeBatteryLevelTask(uint freshnessTime)
        {
            if (IsBackgroundTaskRegistered(TimeBatteryLevelTaskName)) return true;

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;
            
            var trigger = new TimeTrigger(freshnessTime, false);

            var builder = new BackgroundTaskBuilder
            {
                Name = TimeBatteryLevelTaskName
            };
            builder.SetTrigger(trigger);
            var result = builder.Register();

            return result != null;
        }

        public static bool RegisterUserNotificationTask()
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

        public static void Unregister(string taskName)
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
