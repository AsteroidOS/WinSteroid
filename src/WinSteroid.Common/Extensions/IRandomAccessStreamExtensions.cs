using System;
using System.Threading.Tasks;

namespace Windows.Storage.Streams
{
    public static class IRandomAccessStreamExtensions
    {
        public static async Task<byte[]> ToByteArrayAsync(this IRandomAccessStream randomAccessStream)
        {
            var reader = new DataReader(randomAccessStream.GetInputStreamAt(0));

            var bytes = new byte[randomAccessStream.Size];

            await reader.LoadAsync((uint)randomAccessStream.Size);

            reader.ReadBytes(bytes);

            return bytes;
        }
    }
}
