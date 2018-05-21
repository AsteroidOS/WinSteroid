using System;
using Windows.Storage.Streams;

namespace WinSteroid.Common.Helpers
{
    public static class BatteryHelper
    {
        public static int GetPercentage(IBuffer buffer)
        {
            var bytes = new byte[buffer.Length];
            DataReader.FromBuffer(buffer).ReadBytes(bytes);

            return Convert.ToInt32(bytes[0]);
        }
    }
}
