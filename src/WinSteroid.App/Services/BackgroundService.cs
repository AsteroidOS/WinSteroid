using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace WinSteroid.App.Services
{
    public class BackgroundService
    {
        public const string TaskName = "AsteroidBackgroundTask";

        public void Register()
        {
            if (BackgroundTaskRegistration.AllTasks.Any(kvp => kvp.Value.Name.Equals(TaskName, StringComparison.OrdinalIgnoreCase))) return;

            var builder = new BackgroundTaskBuilder { Name = TaskName };
            builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
            builder.Register();
        }

        public void Unregister()
        {
            var tasks = BackgroundTaskRegistration.AllTasks.Where(kvp => !kvp.Value.Name.Equals(TaskName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (!tasks.Any()) return;

            foreach (var task in tasks)
            {
                task.Value.Unregister(cancelTask: true);
            }
        }
    }
}
