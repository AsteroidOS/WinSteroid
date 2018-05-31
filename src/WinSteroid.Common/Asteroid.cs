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
using WinSteroid.Common.Models;

namespace WinSteroid.Common
{
    public static class Asteroid
    {
        //Advertisement UUIDs
        public static Guid AdvertisementUuid = Guid.Parse("00000000-0000-0000-0000-00a57e401d05");
        
        private static BLEService _batteryService;
        public static BLEService BatteryService
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
        public static Guid MediaServiceUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        public static Guid MediaTitleCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        public static Guid AlbumCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        public static Guid ArtistCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        public static Guid PlayingCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");
        public static Guid CommandCharacteristicUuid = Guid.Parse("00007071-0000-0000-0000-00A57E401D05");

        private static BLEService _mediaService;
        public static BLEService MediaService
        {
            get
            {
                if (_mediaService == null)
                {
                    var characteristics = new[]
                    {
                        new BLECharacteristic(nameof(MediaTitleCharacteristicUuid), MediaTitleCharacteristicUuid),
                        new BLECharacteristic(nameof(AlbumCharacteristicUuid), AlbumCharacteristicUuid),
                        new BLECharacteristic(nameof(ArtistCharacteristicUuid), ArtistCharacteristicUuid),
                        new BLECharacteristic(nameof(PlayingCharacteristicUuid), PlayingCharacteristicUuid),
                        new BLECharacteristic(nameof(CommandCharacteristicUuid), CommandCharacteristicUuid)
                    };

                    _mediaService = new BLEService(nameof(MediaServiceUuid), MediaServiceUuid, characteristics);
                }

                return _mediaService;
            }
        }

        //Notifications UUIDs
        public static Guid NotificationServiceUuid = Guid.Parse("00009071-0000-0000-0000-00A57E401D05");
        public static Guid NotificationUpdateCharacteristicUuid = Guid.Parse("00009001-0000-0000-0000-00A57E401D05");
        public static Guid NotificationFeedbackCharacteristicUuid = Guid.Parse("00009002-0000-0000-0000-00A57E401D05");

        private static BLEService _notificationService;
        public static BLEService NotificationService
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
        public static Guid TimeServiceUuid = Guid.Parse("00005071-0000-0000-0000-00A57E401D05");
        public static Guid TimeSetCharacteristicUuid = Guid.Parse("00005001-0000-0000-0000-00A57E401D05");

        private static BLEService _timeService;
        public static BLEService TimeService
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
        public static Guid WeatherServiceUuid = Guid.Parse("00008071-0000-0000-0000-00A57E401D05");
        public static Guid WeatherCityCharacteristicUuid = Guid.Parse("00009001-0000-0000-0000-00A57E401D05");
        public static Guid WeatherIDsCharacteristicUuid = Guid.Parse("00008002-0000-0000-0000-00A57E401D05");
        public static Guid WeatherMinTemperaturesCharacteristicUuid = Guid.Parse("00008003-0000-0000-0000-00A57E401D05");
        public static Guid WeatherMaxTemperaturesCharacteristicUuid = Guid.Parse("00008004-0000-0000-0000-00A57E401D05");

        private static BLEService _weatherService;
        public static BLEService WeatherService
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
        public static Guid ScreenshotsServiceUuid = Guid.Parse("00006071-0000-0000-0000-00A57E401D05");
        public static Guid ScreenshotRequestCharacteristicUuid = Guid.Parse("00006001-0000-0000-0000-00A57E401D05");
        public static Guid ScreenshotContentCharacteristicUuid = Guid.Parse("00006002-0000-0000-0000-00A57E401D05");

        private static BLEService _screenshotsService;
        public static BLEService ScreenshotsService
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

        public static BLEService[] Services
        {
            get
            {
                return new[] { BatteryService, MediaService, NotificationService, TimeService, WeatherService, ScreenshotsService };
            }
        }
    }
}
