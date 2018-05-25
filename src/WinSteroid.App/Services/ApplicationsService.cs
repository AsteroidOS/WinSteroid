﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Services
{
    public class ApplicationsService
    {
        public List<ApplicationPreference> UserIcons;

        public ApplicationsService()
        {
            this.Initialize();
        }

        private async void Initialize()
        {
            var userIconsFile = await GetUserIconsFileAsync();
            var userJson = await FileIO.ReadTextAsync(userIconsFile);
            if (string.IsNullOrWhiteSpace(userJson))
            {
                UserIcons = new List<ApplicationPreference>();
                return;
            }

            UserIcons = JsonConvert.DeserializeObject<List<ApplicationPreference>>(userJson);
        }

        private IAsyncOperation<StorageFile> GetUserIconsFileAsync()
        {
            return ApplicationData.Current.RoamingFolder.CreateFileAsync("userIcons.json", CreationCollisionOption.OpenIfExists);
        }

        public ApplicationPreference GetApplicationPreferenceByAppId(string appId)
        {
            return UserIcons.FirstOrDefault(ui => ui.AppId == appId);
        }

        public ApplicationIcon GetDefaultApplicationIcon() => ApplicationIcon.Alert;

        public void UpsertUserIcon(string appId, string packageName, ApplicationIcon? icon = null, bool muted = false, VibrationLevel vibrationLevel = VibrationLevel.None)
        {
            var existingUserIcon = UserIcons.SingleOrDefault(ui => ui.AppId == appId);
            if (existingUserIcon != null && icon != null)
            {
                existingUserIcon.PackageName = packageName;
                existingUserIcon.Icon = icon ?? ApplicationIcon.Alert;
                existingUserIcon.Muted = muted;
                existingUserIcon.Vibration = muted ? VibrationLevel.None : vibrationLevel;
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

        public async Task SaveUserIcons()
        {
            var json = JsonConvert.SerializeObject(UserIcons);

            var userIconsFile = await GetUserIconsFileAsync();

            await FileIO.WriteTextAsync(userIconsFile, json);

            ApplicationData.Current.SignalDataChanged();
        }

        public Task DeleteUserIcons()
        {
            this.UserIcons = new List<ApplicationPreference>();

            return this.SaveUserIcons();
        }

        public async Task UpsertFoundApplicationsAsync(IEnumerable<UserNotification> userNotifications)
        {
            if (userNotifications == null) return;

            var newAppNotifications = userNotifications.Where(notification => UserIcons.All(ui => ui.AppId != notification.AppInfo.Id)).ToArray();
            if (newAppNotifications?.Length > 0)
            {
                foreach (var notification in newAppNotifications)
                {
                    UpsertUserIcon(notification.AppInfo.PackageFamilyName, notification.AppInfo.DisplayInfo.DisplayName);
                }

                await SaveUserIcons();
            }
        }

        public async Task<bool> ExportDataAsync()
        {
            var folder = await FilesHelper.PickFolderAsync("ExportFolderToken");
            if (folder == null) return false;

            var fileName = $"userIcons_export_{DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.json";

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            var json = JsonConvert.SerializeObject(this.UserIcons);

            await FileIO.WriteTextAsync(file, json);

            return true;
        }

        public IEnumerable<ViewModels.ApplicationViewModel> MapApplications()
        {
            return this.UserIcons
                .OrderBy(ui => ui.PackageName)
                .Select(ui => new ViewModels.ApplicationViewModel
                {
                    Id = ui.AppId,
                    Name = ui.PackageName,
                    Icon = ui.Icon,
                    Muted = ui.Muted,
                    HasVibration = ui.Muted ? false : ui.Vibration != VibrationLevel.None
                })
                .ToArray();
        }
    }
}
