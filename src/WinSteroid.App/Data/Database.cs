using SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;

namespace WinSteroid.App.Data
{
    public class Database
    {
        private readonly SQLiteAsyncConnection SQLiteDbContext;
        private const string DatabaseName = "app.db";

        public Database()
        {
            var databasePath = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName));
            this.SQLiteDbContext = new SQLiteAsyncConnection(databasePath);
            this.SQLiteDbContext.CreateTableAsync<Models.Notification>();
        }

        public Task<int> InsertNotificationAsync(UserNotification userNotification)
        {
            var notification = userNotification.AsNotification();

            return this.SQLiteDbContext.InsertAsync(notification);
        }

        public async Task<bool> ExistsNotificationWithId(string id)
        {
            var count = await this.SQLiteDbContext.Table<Models.Notification>().Where(n => n.Id == id).CountAsync();
            return count == 1;
        }

        public async Task MarkNotificationsAsNotifiedAsync(string[] ids)
        {
            var notifications = await this.SQLiteDbContext.Table<Models.Notification>().Where(n => ids.Any(id => n.Id == id)).ToListAsync();
            if (!notifications.Any()) return;

            foreach (var notification in notifications)
            {
                notification.Notified = true;
            }

            await this.SQLiteDbContext.UpdateAllAsync(notifications);
        }
    }
}
