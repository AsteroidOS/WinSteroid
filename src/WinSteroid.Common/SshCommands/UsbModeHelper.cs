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
using System.Collections.Generic;
using System.Linq;
using WinSteroid.Common.Models;

namespace WinSteroid.Common.SshCommands
{
    public static class UsbModeHelper
    {
        public static IEnumerable<UsbMode> ParseModes(string output)
        {
            if (!output.Contains('='))
            {
                throw new ArgumentException(nameof(output));
            }

            var strings = output.Split('=')[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in strings)
            {
                yield return Parse(s);
            }
        }

        public static UsbMode Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (s.Contains('='))
            {
                s = s.Split('=')[1].Trim();
            }

            switch (s.Trim())
            {
                case "charging_only":
                    return new UsbMode { Key = s, TypeMode = UsbModeEnum.ChargingOnly };
                case "developer_mode":
                    return new UsbMode { Key = s, TypeMode = UsbModeEnum.Developer };
                case "mtp_mode":
                    return new UsbMode { Key = s, TypeMode = UsbModeEnum.MTP };
                case "adb_mode":
                    return new UsbMode { Key = s, TypeMode = UsbModeEnum.ADB };
            }

            throw new NotSupportedException($"Unsupported {nameof(UsbModeEnum)}: {s}");
        }
    }
}
