using System;
using Windows.Storage.Streams;
using WinSteroid.Common.Models;

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

        public static string GetIcon(int percentage)
        {
            switch (percentage)
            {
                case var p when percentage >= (int)BatteryLevel.Discrete:
                    {
                        return "";
                    }
                case var p when percentage < (int)BatteryLevel.Discrete && percentage >= (int)BatteryLevel.Bad:
                    {
                        return "";
                    }
                case var p when percentage < (int)BatteryLevel.Bad && percentage >= (int)BatteryLevel.Critic:
                    {
                        return "";
                    }
            }

            return "";
        }

        public static BatteryLevel Parse(int percentage)
        {
            switch (percentage)
            {
                case var p when percentage >= (int)BatteryLevel.Discrete:
                    {
                        return BatteryLevel.Good;
                    }
                case var p when percentage < (int)BatteryLevel.Discrete && percentage >= (int)BatteryLevel.Bad:
                    {
                        return BatteryLevel.Discrete;
                    }
                case var p when percentage < (int)BatteryLevel.Bad && percentage >= (int)BatteryLevel.Critic:
                    {
                        return BatteryLevel.Bad;
                    }
                case var p when percentage < (int)BatteryLevel.Critic && percentage >= 0:
                    {
                        return BatteryLevel.Critic;
                    }
            }

            return BatteryLevel.Dead;
        }
    }
}
