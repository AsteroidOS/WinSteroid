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
