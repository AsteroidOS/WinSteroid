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
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace WinSteroid.Common.Helpers
{
    public static class ImageHelper
    {
        private const int DefaultAppLogoSize = 40;
        
        public static async Task<BitmapImage> ConvertToImageAsync(byte[] bytes)
        {
            using (var ras = new InMemoryRandomAccessStream())
            {
                var memoryStream = new MemoryStream(bytes);

                await memoryStream.CopyToAsync(ras.AsStreamForWrite());

                var bitmapImage = new BitmapImage();

                await bitmapImage.SetSourceAsync(ras);

                return bitmapImage;
            }
        }

        public static async Task<BitmapImage> ConvertToImageAsync(AppDisplayInfo appDisplayInfo)
        {
            try
            {
                using (var ras = await appDisplayInfo.GetLogo(new Size(DefaultAppLogoSize, DefaultAppLogoSize)).OpenReadAsync())
                {
                    var bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(ras);

                    return bitmapImage;
                }
            }
            catch
            {
                var fallbackLogo = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Square44x44Logo.png"));
                using (var ras = await fallbackLogo.OpenReadAsync())
                {
                    var bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(ras);

                    return bitmapImage;
                }
            }
        }

        public static async Task<byte[]> EncodeToSquareJpegImageAsync(StorageFile inputFile, uint size, double dpi = 72)
        {
            var inputStream = await inputFile.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(inputStream);

            using (var inMemoryRandomAccessStream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateForTranscodingAsync(inMemoryRandomAccessStream, decoder);
                encoder.BitmapTransform.ScaledWidth = size;
                encoder.BitmapTransform.ScaledHeight = size;

                await encoder.FlushAsync();
                
                await inMemoryRandomAccessStream.FlushAsync();

                return await inMemoryRandomAccessStream.ToByteArrayAsync();
            }
        }

        public async static Task<StorageFile> CropImageFileAsync(StorageFile inputFile, int cropWidthPixels, int cropHeightPixels)
        {
            var outputFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("Cropped_" + inputFile.Name, CreationCollisionOption.ReplaceExisting);

            var inputToken = SharedStorageAccessManager.AddFile(inputFile);
            var destinationToken = SharedStorageAccessManager.AddFile(outputFile);

            var inputData = new ValueSet
            {
                { "InputToken", inputToken },
                { "DestinationToken", destinationToken },
                { "CropWidthPixels", cropWidthPixels },
                { "CropHeightPixels", cropHeightPixels },
                { "EllipticalCrop", false }
            };

            var launcherOptions = new LauncherOptions
            {
                TargetApplicationPackageFamilyName = "Microsoft.Windows.Photos_8wekyb3d8bbwe"
            };

            var launcherResult = await Launcher.LaunchUriForResultsAsync(new Uri("microsoft.windows.photos.crop:"), launcherOptions, inputData);
            if (launcherResult.Status != LaunchUriStatus.Success) return null;

            return outputFile;
        }
    }
}