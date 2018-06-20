//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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

        public void UpsertUserIcon(ApplicationPreference applicationPreference)
        {
            this.UpsertUserIcon(applicationPreference.AppId, applicationPreference.PackageName, applicationPreference.Icon, applicationPreference.Muted, applicationPreference.Vibration);
        }

        public void UpsertUserIcon(string appId, string packageName, ApplicationIcon? icon = null, bool muted = false, VibrationLevel vibrationLevel = VibrationLevel.None)
        {
            var existingUserIcon = UserIcons.SingleOrDefault(ui => ui.AppId == appId);
            if (existingUserIcon != null && icon != null)
            {
                existingUserIcon.PackageName = packageName;
                existingUserIcon.Icon = icon.GetValueOrDefault();
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

        public async Task<ApplicationPreference[]> ImportDataAsync()
        {
            var file = await FilesHelper.PickFileAsync(".json");
            if (file == null) return new ApplicationPreference[0];

            var json = await FileIO.ReadTextAsync(file);
            if (string.IsNullOrWhiteSpace(json))
            {
                //EMPTY FILE
                throw new ArgumentException(ResourcesHelper.GetLocalizedString("ApplicationsServiceImportDataEmptyFileError"));
            }

            var applicationPreferences = JsonConvert.DeserializeObject<ApplicationPreference[]>(json);
            if (applicationPreferences == null)
            {
                //INVALID FORMAT
                throw new ArgumentException(ResourcesHelper.GetLocalizedString("ApplicationsServiceImportDataInvalidFileError"));
            }

            return applicationPreferences;
        }

        public async Task<bool> ExportDataAsync()
        {
            var folder = await FilesHelper.PickFolderAsync("ExportFolderToken");
            if (folder == null) return false;

            var applicationName = Package.Current.DisplayName.Replace(" ", "_");

            var fileName = $"{applicationName}_userIcons_export_{DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.json";

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            var json = JsonConvert.SerializeObject(this.UserIcons);

            await FileIO.WriteTextAsync(file, json);

            return true;
        }
    }
}
