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

namespace WinSteroid.Common.Models
{
    public static class ApplicationIconExtensions
    {
        public static List<ApplicationIcon> GetList()
        {
            return EnumExtensions.GetValues<ApplicationIcon>().OrderBy(ai => (int)ai).ToList();
        }

        public static string GetId(this ApplicationIcon applicationIcon)
        {
            switch (applicationIcon)
            {
                case ApplicationIcon.Alert:
                default:
                    return "ios-alert";
                case ApplicationIcon.Apps:
                    return "ios-apps";
                case ApplicationIcon.Calendar:
                    return "ios-calendar";
                case ApplicationIcon.Call:
                    return "ios-calendar";
                case ApplicationIcon.Message:
                    return "ios-mail";
                case ApplicationIcon.Photo:
                    return "ios-camera";
                case ApplicationIcon.Settings:
                    return "ios-settings";
                case ApplicationIcon.SMS:
                    return "ios-send";
                case ApplicationIcon.Money:
                    return "ios-pricetags-outline";
                case ApplicationIcon.Food:
                    return "ios-pizza";
            }
        }

        public static string GetGlyph(this ApplicationIcon applicationIcon)
        {
            switch (applicationIcon)
            {
                case ApplicationIcon.Alert:
                default:
                    return "";
                case ApplicationIcon.Apps:
                    return "";
                case ApplicationIcon.Calendar:
                    return "";
                case ApplicationIcon.Call:
                    return "";
                case ApplicationIcon.Message:
                    return "";
                case ApplicationIcon.Photo:
                    return "";
                case ApplicationIcon.Settings:
                    return "";
                case ApplicationIcon.SMS:
                    return "";
                case ApplicationIcon.Money:
                    return "";
                case ApplicationIcon.Food:
                    return "";
            }
        }
    }
}
