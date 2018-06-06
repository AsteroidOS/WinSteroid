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
            using (var ras = await appDisplayInfo.GetLogo(new Size(DefaultAppLogoSize, DefaultAppLogoSize)).OpenReadAsync())
            {
                var bitmapImage = new BitmapImage();

                await bitmapImage.SetSourceAsync(ras);

                return bitmapImage;
            }
        }

        public static async Task<byte[]> EncodeToSquareJpegImageAsync(StorageFile inputFile, uint size, double dpi = 72)
        {
            var pixelsBytes = new byte[0];

            using (var inputStream = await inputFile.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);
                var pixelData = await decoder.GetPixelDataAsync();
                pixelsBytes = pixelData.DetachPixelData();
            }

            using (var inMemoryRandomAccessStream = new InMemoryRandomAccessStream())
            {
                var bitmapPropertySet = new BitmapPropertySet();
                var bitmapTypedValue = new BitmapTypedValue(1.0d, PropertyType.Single);
                bitmapPropertySet.Add("ImageQuality", bitmapTypedValue);

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, inMemoryRandomAccessStream, bitmapPropertySet);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, size, size, dpi, dpi, pixelsBytes);

                await encoder.FlushAsync();

                await inMemoryRandomAccessStream.FlushAsync();

                return await inMemoryRandomAccessStream.ToByteArrayAsync();
            }
        }

        public static Task<StorageFile> CropImageFileAsync(StorageFile storageFile, int cropWidthPixels, int cropHeightPixels, bool ellipticalCrop)
        {
            throw new NotImplementedException();
            //var destinationToken = SharedStorageAccessManager.AddFile(storageFile);

            //var inputData = new ValueSet
            //{
            //    { "CropWidthPixels", cropWidthPixels },
            //    { "CropHeightPixels", cropHeightPixels },
            //    { "EllipticalCrop", ellipticalCrop },
            //    { "ShowCamera", false },
            //    { "DestinationToken", destinationToken }
            //};

            //var launcherOptions = new LauncherOptions
            //{
            //    TargetApplicationPackageFamilyName = PhotosApplicationPackageName
            //};

            //var launcherResult = await Launcher.LaunchUriForResultsAsync(new Uri(PhotosApplicationCropUri), launcherOptions, inputData);
            //if (launcherResult.Status != LaunchUriStatus.Success) return null;

            //return storageFile;
        }
    }
}