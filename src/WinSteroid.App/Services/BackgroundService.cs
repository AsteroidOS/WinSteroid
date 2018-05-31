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
using WinSteroid.Common;

namespace WinSteroid.App.Services
{
    public class BackgroundService
    {
        private readonly DeviceService DeviceService;

        public BackgroundService(DeviceService deviceService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        }

        public const string BatteryLevelTaskName = "BatteryLevelBackgroundTask";
        public const string ActiveNotificationTaskName = "ActiveNotificationBackgroundTask";
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
