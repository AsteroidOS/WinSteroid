using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Storage.Streams;
using WinSteroid.Common.Helpers;

namespace WinSteroidTasks
{
    public sealed class ActiveNotificationBackgroundTask : IBackgroundTask
    {
        private IBackgroundTaskInstance BackgroundTaskInstance;
        private BackgroundTaskDeferral BackgroundTaskDeferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.BackgroundTaskDeferral = taskInstance.GetDeferral();
            this.BackgroundTaskInstance = taskInstance;
            this.BackgroundTaskInstance.Canceled += OnBackgroundTaskInstanceCanceled;

            var details = (GattCharacteristicNotificationTriggerDetails)BackgroundTaskInstance.TriggerDetails;

            var bytes = new byte[details.Value.Length];

            DataReader.FromBuffer(details.Value).ReadBytes(bytes);

            if (bytes?.Length > 0)
            {
                var @string = Encoding.UTF8.GetString(bytes);
                ToastsHelper.Show("Test action", @string);
            }

            this.BackgroundTaskDeferral.Complete();
        }

        private void OnBackgroundTaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            throw new NotImplementedException();
        }
    }
}
