using SQLite;
using System.Collections.Generic;
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

        public Task<int> InsertNotificationAsync(UserNotification userNotification, bool notified)
        {
            var notification = userNotification.AsNotification(notified);

            return this.SQLiteDbContext.InsertAsync(notification);
        }

        public Task<Models.Notification> RetrieveNotificationWithId(string id)
        {
            return this.SQLiteDbContext.Table<Models.Notification>().Where(n => n.Id == id).FirstOrDefaultAsync();
        }

        public Task<List<Models.Notification>> RetrieveAllNotifications()
        {
            return this.SQLiteDbContext.Table<Models.Notification>().ToListAsync();
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

        public async Task RemoveNotificationAsync(string id)
        {
            var item = await this.SQLiteDbContext.Table<Models.Notification>().Where(n => n.Id == id).FirstOrDefaultAsync();
            if (item == null) return;

            await this.SQLiteDbContext.DeleteAsync(item);
        }
    }
}
