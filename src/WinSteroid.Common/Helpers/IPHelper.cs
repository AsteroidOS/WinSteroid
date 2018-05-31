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

namespace WinSteroid.Common.Helpers
{
    public class IPHelper
    {
        public static bool IsValid(string hostIp)
        {
            if (string.IsNullOrWhiteSpace(hostIp)) return false;

            var segments = hostIp.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.IsNullOrEmpty() || segments.Length != 4) return false;

            foreach (var segment in segments)
            {
                if (!byte.TryParse(segment, out byte n)) return false;
            }

            return true;
        }
    }
}
