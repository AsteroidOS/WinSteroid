using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace WinSteroid.Common.Helpers
{
    public static class ImageHelper
    {
        public static async Task<BitmapImage> ConvertToImage(byte[] bytes)
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
    }
}
