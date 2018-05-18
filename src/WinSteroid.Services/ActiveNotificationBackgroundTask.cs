using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Storage.Streams;
using WinSteroid.Services.Helpers;

namespace WinSteroid.Services
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

            byte[] receivedData = null;

            using (var dataReader = DataReader.FromBuffer(details.Value))
            {
                receivedData = new byte[dataReader.UnconsumedBufferLength];
                dataReader.ReadBytes(receivedData);
            }

            if (receivedData?.Length > 0)
            {
                var @string = Encoding.UTF8.GetString(receivedData);
                ToastsHelper.Send("Test action", @string);
            }

            this.BackgroundTaskDeferral.Complete();
        }

        private void OnBackgroundTaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            throw new NotImplementedException();
        }
    }
}
