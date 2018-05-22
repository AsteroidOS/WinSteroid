using Windows.ApplicationModel.Background;

namespace WinSteroidTasks
{
    public sealed class SystemSessionConnectedTask : IBackgroundTask
    {
        private IBackgroundTaskInstance BackgroundTaskInstance;
        private BackgroundTaskDeferral BackgroundTaskDeferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.BackgroundTaskDeferral = taskInstance.GetDeferral();
            this.BackgroundTaskInstance = taskInstance;

            this.BackgroundTaskDeferral.Complete();
        }
    }
}
