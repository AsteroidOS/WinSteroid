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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.Shared.Models;

namespace WinSteroid.Shared
{
    internal static class Asteroid
    {
        //Advertisement UUIDs
        internal static Guid AdvertisementServiceUuid = Guid.Parse("00000000-0000-0000-0000-00a57e401d05");

        private static BLEService _batteryService;
        internal static BLEService BatteryService
        {
            get
            {
                if (_batteryService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(GattCharacteristicUuids.BatteryLevel), GattCharacteristicUuids.BatteryLevel)
                    };

                    _batteryService = new BLEService(nameof(GattServiceUuids.Battery), GattServiceUuids.Battery, characteristics);
                }

                return _batteryService;
            }
        }

        //Media UUIDs
        internal static Guid MediaServiceUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        internal static Guid MediaTitleCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        internal static Guid MediaAlbumCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        internal static Guid MediaArtistCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        internal static Guid MediaPlayingCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        internal static Guid MediaCommandCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");

        private static BLEService _mediaService;
        internal static BLEService MediaService
        {
            get
            {
                if (_mediaService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(MediaTitleCharacteristicUuid), MediaTitleCharacteristicUuid),
                        new BLECharacteristic(nameof(MediaAlbumCharacteristicUuid), MediaAlbumCharacteristicUuid),
                        new BLECharacteristic(nameof(MediaArtistCharacteristicUuid), MediaArtistCharacteristicUuid),
                        new BLECharacteristic(nameof(MediaPlayingCharacteristicUuid), MediaPlayingCharacteristicUuid),
                        new BLECharacteristic(nameof(MediaCommandCharacteristicUuid), MediaCommandCharacteristicUuid)
                    };

                    _mediaService = new BLEService(nameof(MediaServiceUuid), MediaServiceUuid, characteristics);
                }

                return _mediaService;
            }
        }

        //Notifications UUIDs
        internal static Guid NotificationServiceUuid = Guid.Parse("00009071-0000-0000-0000-00A57E401D05");
        internal static Guid NotificationUpdateCharacteristicUuid = Guid.Parse("00009001-0000-0000-0000-00A57E401D05");
        internal static Guid NotificationFeedbackCharacteristicUuid = Guid.Parse("00009002-0000-0000-0000-00A57E401D05");

        private static BLEService _notificationService;
        internal static BLEService NotificationService
        {
            get
            {
                if (_notificationService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(NotificationUpdateCharacteristicUuid), NotificationUpdateCharacteristicUuid),
                        new BLECharacteristic(nameof(NotificationFeedbackCharacteristicUuid), NotificationFeedbackCharacteristicUuid)
                    };

                    _notificationService = new BLEService(nameof(NotificationServiceUuid), NotificationServiceUuid, characteristics);
                }

                return _notificationService;
            }
        }

        //Time UUIDs
        internal static Guid TimeServiceUuid = Guid.Parse("00005071-0000-0000-0000-00A57E401D05");
        internal static Guid TimeSetCharacteristicUuid = Guid.Parse("00005001-0000-0000-0000-00A57E401D05");

        private static BLEService _timeService;
        internal static BLEService TimeService
        {
            get
            {
                if (_timeService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(TimeSetCharacteristicUuid), TimeSetCharacteristicUuid)
                    };

                    _timeService = new BLEService(nameof(TimeServiceUuid), TimeServiceUuid, characteristics);
                }

                return _timeService;
            }
        }

        //Weather UUIDs
        internal static Guid WeatherServiceUuid = Guid.Parse("00008071-0000-0000-0000-00A57E401D05");
        internal static Guid WeatherCityCharacteristicUuid = Guid.Parse("00009001-0000-0000-0000-00A57E401D05");
        internal static Guid WeatherIDsCharacteristicUuid = Guid.Parse("00008002-0000-0000-0000-00A57E401D05");
        internal static Guid WeatherMinTemperaturesCharacteristicUuid = Guid.Parse("00008003-0000-0000-0000-00A57E401D05");
        internal static Guid WeatherMaxTemperaturesCharacteristicUuid = Guid.Parse("00008004-0000-0000-0000-00A57E401D05");

        private static BLEService _weatherService;
        internal static BLEService WeatherService
        {
            get
            {
                if (_weatherService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(WeatherCityCharacteristicUuid), WeatherCityCharacteristicUuid),
                        new BLECharacteristic(nameof(WeatherIDsCharacteristicUuid), WeatherIDsCharacteristicUuid),
                        new BLECharacteristic(nameof(WeatherMinTemperaturesCharacteristicUuid), WeatherMinTemperaturesCharacteristicUuid),
                        new BLECharacteristic(nameof(WeatherMaxTemperaturesCharacteristicUuid), WeatherMaxTemperaturesCharacteristicUuid)
                    };

                    _weatherService = new BLEService(nameof(WeatherServiceUuid), WeatherServiceUuid, characteristics);
                }

                return _weatherService;
            }
        }

        //Screenshots UUIDs
        internal static Guid ScreenshotsServiceUuid = Guid.Parse("00006071-0000-0000-0000-00A57E401D05");
        internal static Guid ScreenshotRequestCharacteristicUuid = Guid.Parse("00006001-0000-0000-0000-00A57E401D05");
        internal static Guid ScreenshotContentCharacteristicUuid = Guid.Parse("00006002-0000-0000-0000-00A57E401D05");

        private static BLEService _screenshotsService;
        internal static BLEService ScreenshotsService
        {
            get
            {
                if (_screenshotsService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(ScreenshotRequestCharacteristicUuid), ScreenshotRequestCharacteristicUuid),
                        new BLECharacteristic(nameof(ScreenshotContentCharacteristicUuid), ScreenshotContentCharacteristicUuid)
                    };

                    _screenshotsService = new BLEService(nameof(ScreenshotsServiceUuid), ScreenshotsServiceUuid, characteristics);
                }

                return _screenshotsService;
            }
        }

        internal static BLEService[] Services
        {
            get
            {
                return new[] { BatteryService, MediaService, NotificationService, TimeService, WeatherService, ScreenshotsService };
            }
        }

        internal const int CurrentMinimalBtsyncdPacketSize = 200;

        internal const string DefaultIPv4 = "192.168.2.15";
        internal const string DefaultRootUsername = "root";
    }
}
