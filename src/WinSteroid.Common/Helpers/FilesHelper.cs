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

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace WinSteroid.Common.Helpers
{
    public static class FilesHelper
    {
        public static IAsyncOperation<StorageFolder> GetScreenshotsFolderAsync()
        {
            return ApplicationData.Current.LocalFolder.CreateFolderAsync(Constants.ScreenshotsFolderName, CreationCollisionOption.OpenIfExists);
        }

        public static IAsyncOperation<StorageFile> PickFileAsync(params string[] fileExtensions)
        {
            var fileOpenPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            if (!fileExtensions.IsNullOrEmpty())
            {
                for (var i = 0; i < fileExtensions.Length; i++)
                {
                    if (!fileExtensions[i].StartsWith("."))
                    {
                        fileExtensions[i] = "." + fileExtensions[i];
                    }
                    fileOpenPicker.FileTypeFilter.Add(fileExtensions[i]);
                }
            }

            return fileOpenPicker.PickSingleFileAsync();
        }

        public static async Task<StorageFolder> PickFolderAsync(string folderToken)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                if (string.IsNullOrWhiteSpace(folderToken))
                {
                    folderToken = folder.Name + "Token";
                }

                StorageApplicationPermissions.FutureAccessList.AddOrReplace(folderToken, folder);
            }

            return folder;
        }
    }
}
