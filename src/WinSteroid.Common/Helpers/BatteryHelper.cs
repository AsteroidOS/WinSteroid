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
