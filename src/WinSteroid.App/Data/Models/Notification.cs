using SQLite;
using System;

namespace WinSteroid.App.Data.Models
{
    public class Notification
    {
        [PrimaryKey]
        [Indexed(Unique = false)]
        public string Id { get; set; }

        [Indexed(Unique = false)]
        public DateTime CreatedAt { get; set; }

        [Indexed(Unique = false)]
        public string AppId { get; set; }

        public bool Notified { get; set; }
    }
}
