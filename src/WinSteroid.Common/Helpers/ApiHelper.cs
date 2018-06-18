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

using Windows.Foundation.Metadata;

namespace WinSteroid.Common.Helpers
{
    public static class ApiHelper
    {
        public static bool CheckIfSystemSupportNotificationListener()
        {
            return ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener");
        }

        public static bool CheckIfIsSystemMobile()
        {
            return ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
        }

        public static bool CheckIfIsSystemTrayPresent()
        {
            return ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        }

        public static bool SupportAcrylicBrushes()
        {
            return ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush");
        }
    }
}
