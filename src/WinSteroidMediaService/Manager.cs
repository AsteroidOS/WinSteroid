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
using System.Text;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using WinSteroid.Common;
using WinSteroid.Common.Bluetooth;
using WinSteroid.Common.Helpers;

namespace WinSteroidMediaService
{
    public sealed class Manager : IBackgroundTask
    {
        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceConnection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.backgroundTaskDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskInstanceCanceled;

            this.appServiceConnection = (taskInstance.TriggerDetails as AppServiceTriggerDetails).AppServiceConnection;
            appServiceConnection.RequestReceived += OnAppServiceConnectionRequestReceived;
        }

        void OnTaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.backgroundTaskDeferral != null)
            {
                this.backgroundTaskDeferral.Complete();
            }
        }

        async void OnAppServiceConnectionRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();

            var requestData = args.Request.Message;
            if (!this.IsValidRequestDataSet(requestData, out Guid characteristicUuid, out string data))
            {
                await args.SendResponseAsync(this.GetFailureValueSet(sender.PackageFamilyName, "invalid data"));
                deferral.Complete();
                return;
            }

            var deviceId = SettingsHelper.GetValue(Constants.LastSavedDeviceIdSettingKey, string.Empty);
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                await args.SendResponseAsync(this.GetFailureValueSet(sender.PackageFamilyName, "device not found"));
                deferral.Complete();
                return;
            }

            var errorMessage = await DeviceManager.ConnectAsync(deviceId);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                await args.SendResponseAsync(this.GetFailureValueSet(sender.PackageFamilyName, "connection failed"));
                deferral.Complete();
                return;
            }
            
            var result = await DeviceManager.WriteByteArrayToCharacteristicAsync(characteristicUuid, Encoding.UTF8.GetBytes(data));
            
            await args.SendResponseAsync(this.GetSuccessValueSet(sender.PackageFamilyName));

            await DeviceManager.DisconnectAsync();

            deferral.Complete();
        }

        ValueSet GetFailureValueSet(string packageName, string failureMessage)
        {
            return new ValueSet
            {
                { "packageName", packageName },
                { "success", false },
                { "errorMessage", failureMessage }
            };
        }

        ValueSet GetSuccessValueSet(string packageName)
        {
            return new ValueSet
            {
                { "packageName", packageName },
                { "success", true }
            };
        }
        
        bool IsValidRequestDataSet(ValueSet valueSet, out Guid characteristicUuid, out string data)
        {
            if (valueSet == null)
            {
                characteristicUuid = Guid.Empty;
                data = string.Empty;
                return false;
            }

            if (!valueSet.ContainsKey("uuid") || !Guid.TryParse(valueSet["uuid"] as string, out characteristicUuid))
            {
                characteristicUuid = Guid.Empty;
                data = string.Empty;
                return false;
            }

            if (!valueSet.ContainsKey("data") || valueSet["data"] == null)
            {
                data = string.Empty;
                return false;
            }

            data = valueSet["data"] as string;

            return !string.IsNullOrWhiteSpace(data);
        }
    }
}
