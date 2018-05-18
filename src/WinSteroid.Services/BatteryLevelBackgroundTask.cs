using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Storage.Streams;
using WinSteroid.Services.Helpers;

namespace WinSteroid.Services
{
    public sealed class BatteryLevelBackgroundTask : IBackgroundTask
    {
        private IBackgroundTaskInstance BackgroundTaskInstance;
        private BackgroundTaskDeferral BackgroundTaskDeferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.BackgroundTaskDeferral = taskInstance.GetDeferral();
            this.BackgroundTaskInstance = taskInstance;
            this.BackgroundTaskInstance.Canceled += OnBackgroundTaskInstanceCanceled;

            var details = (GattCharacteristicNotificationTriggerDetails)BackgroundTaskInstance.TriggerDetails;

            byte[] receivedData = null;

            using (var dataReader = DataReader.FromBuffer(details.Value))
            {
                receivedData = new byte[dataReader.UnconsumedBufferLength];
                dataReader.ReadBytes(receivedData);
            }

            if (receivedData?.Length > 0)
            {
                var percentage = Convert.ToUInt16(receivedData[0]);
                this.BackgroundTaskInstance.Progress = percentage;
                TilesHelper.UpdateBatteryTile(percentage);
            }
            else
            {
                TilesHelper.ResetBatteryTile();
            }

            this.BackgroundTaskDeferral.Complete();
        }

        private void OnBackgroundTaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            TilesHelper.ResetBatteryTile();
            ToastsHelper.Send("OPS!", "Something strange happened and I can no more monitor your watch battery level!");
        }
    }
}
