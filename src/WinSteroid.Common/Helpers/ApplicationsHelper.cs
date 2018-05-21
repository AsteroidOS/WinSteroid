using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using WinSteroid.Common.Models;

namespace WinSteroid.Common.Helpers
{
    public static class ApplicationsHelper
    {
        private const string UserIconsFileName = "userIcons.json";
        
        private static ApplicationPreference[] DefaultIcons { get; set; }
        public static List<ApplicationPreference> UserIcons { get; private set; }

        public static bool Initialized() => DefaultIcons != null && DefaultIcons.Length > 0;

        public static async Task InitializeAsync()
        {
            var defaultIconsFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Icons/defaultIcons.json"));
            var defaultJson = await FileIO.ReadTextAsync(defaultIconsFile);
            DefaultIcons = JsonConvert.DeserializeObject<ApplicationPreference[]>(defaultJson);

            var userIconsFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(UserIconsFileName, CreationCollisionOption.ReplaceExisting);
            var userJson = await FileIO.ReadTextAsync(userIconsFile);
            if (string.IsNullOrWhiteSpace(userJson))
            {
                UserIcons = DefaultIcons.ToList();
                return;
            }

            UserIcons = JsonConvert.DeserializeObject<List<ApplicationPreference>>(userJson);
        }

        public static ApplicationPreference GetApplicationPreferenceByAppId(string appId)
        {
            return UserIcons.FirstOrDefault(ui => ui.AppId == appId);
        }

        public static ApplicationIcon GetDefaultApplicationIcon() => ApplicationIcon.Alert;

        public static void UpsertUserIcon(string appId, string packageName, ApplicationIcon? icon = null, bool muted = false, VibrationLevel vibrationLevel = VibrationLevel.None)
        {
            var existingUserIcon = UserIcons.SingleOrDefault(ui => ui.AppId == appId);
            if (existingUserIcon != null && icon != null)
            {
                existingUserIcon.PackageName = packageName;
                existingUserIcon.Icon = icon ?? ApplicationIcon.Alert;
            }
            else if (existingUserIcon == null)
            {
                if (icon == null)
                {
                    icon = GetDefaultApplicationIcon();
                }

                var userIcon = new ApplicationPreference
                {
                    AppId = appId,
                    PackageName = packageName,
                    Icon = icon.GetValueOrDefault(),
                    Muted = muted,
                    Vibration = muted ? VibrationLevel.None : vibrationLevel
                };

                UserIcons.Add(userIcon);
            }
        }

        public static async Task SaveUserIcons()
        {
            var json = JsonConvert.SerializeObject(UserIcons);

            var userIconsFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(UserIconsFileName, CreationCollisionOption.OpenIfExists);

            await FileIO.WriteTextAsync(userIconsFile, json);

            ApplicationData.Current.SignalDataChanged();
        }

        public static Task UpsertFoundApplicationsAsync(IEnumerable<UserNotification> userNotifications)
        {
            if (userNotifications == null) return Task.CompletedTask;

            var newAppNotifications = userNotifications.Where(notification => UserIcons.All(ui => ui.AppId != notification.AppInfo.Id)).ToArray();
            if (newAppNotifications?.Length > 0)
            {
                foreach (var notification in newAppNotifications)
                {
                    UpsertUserIcon(notification.AppInfo.Id, notification.AppInfo.DisplayInfo.DisplayName);
                }

                return SaveUserIcons();
            }

            return Task.CompletedTask;
        }

        public static async Task ExportDataAsync()
        {
            var folderPicker = new FolderPicker();
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;

            StorageApplicationPermissions.FutureAccessList.AddOrReplace("ExportFolderToken", folder);

            var exportFolder = await folder.CreateFolderAsync(nameof(WinSteroid), CreationCollisionOption.OpenIfExists);

            var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(UserIconsFileName, CreationCollisionOption.OpenIfExists);

            await file.CopyAsync(exportFolder);
        }
    }
}
