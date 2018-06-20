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
using Windows.ApplicationModel.Resources;

namespace WinSteroid.Common.Helpers
{
    public static class ResourcesHelper
    {
        private static ResourceLoader _resourceLoader;
        private static ResourceLoader ResourceLoader
        {
            get
            {
                if (_resourceLoader == null)
                {
                    var resourceLoader = ResourceLoader.GetForCurrentView();
                    _resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
                }

                return _resourceLoader;
            }
        }

        public static string GetLocalizedString(string resourceKey)
        {
            return ResourceLoader.GetString(resourceKey);
        }
    }
}
