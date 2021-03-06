﻿//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Converters
{
    public class BatteryLevelToVisibilityConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BatteryLevel batteryLevel && parameter is int index)
            {
                switch (batteryLevel)
                {
                    case BatteryLevel.Good:
                    case BatteryLevel.Discrete:
                        return Visibility.Visible;
                    case BatteryLevel.Bad:
                        return index < 3 ? Visibility.Visible : Visibility.Collapsed;
                    case BatteryLevel.Critic:
                        return index < 2 ? Visibility.Visible : Visibility.Collapsed;
                    case BatteryLevel.Dead:
                        return index < 1 ? Visibility.Visible : Visibility.Collapsed;
                    default:
                        return Visibility.Collapsed;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
