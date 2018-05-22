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
