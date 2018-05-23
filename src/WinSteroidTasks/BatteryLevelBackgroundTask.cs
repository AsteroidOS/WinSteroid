using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using WinSteroid.Common.Helpers;

namespace WinSteroidTasks
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

            var percentage = BatteryHelper.GetPercentage(details.Value);
            if (percentage > 0)
            {
                TilesHelper.UpdateBatteryTile(percentage);
            }
            else
            {
                TilesHelper.ResetBatteryTile();
            }

            this.BackgroundTaskInstance.Progress = Convert.ToUInt32(percentage);
            this.BackgroundTaskDeferral.Complete();
        }

        private void OnBackgroundTaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            TilesHelper.ResetBatteryTile();
            ToastsHelper.Show("OPS!", "Something strange happened and I can no more monitor your watch battery level!");
        }
    }
}
