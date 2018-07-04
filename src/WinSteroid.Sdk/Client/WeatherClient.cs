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
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using WinSteroid.Shared;
using WinSteroid.Shared.Models;

namespace WinSteroid.Sdk.Client
{
    public class WeatherClient
    {
        #region Constants

        const int MaxWeatherDataSize = 5;

        const string MaxWeatherDataSizeErrorMessage = "You cannot send a size of data not equal to 10 bytes (5 days)";

        #endregion

        #region Fields

        AppServiceConnection AppServiceConnection;

        #endregion

        #region Public methods

        public async Task<bool> SendGenericDataUpdateByDataTypeAsync(byte[] data, WeatherDataType dataType)
        {
            if (this.AppServiceConnection == null)
            {
                await this.InitializeAsync();
            }

            var message = CreateMessage(GetCharacteristicUuidByWeatherDataType(dataType), data);

            var errorMessage = await this.AppServiceConnection.SendMessageWithResponseAsync(message);

            return string.IsNullOrWhiteSpace(errorMessage);
        }

        public async Task<bool> SendCityDataAsync(string city)
        {
            if (this.AppServiceConnection == null)
            {
                await this.InitializeAsync();
            }

            var data = Encoding.UTF8.GetBytes(city);

            var message = CreateMessage(Asteroid.WeatherCityCharacteristicUuid, data);

            var errorMessage = await this.AppServiceConnection.SendMessageWithResponseAsync(message);

            return string.IsNullOrWhiteSpace(errorMessage);
        }

        public async Task<bool> SendIDsDataAsync(IEnumerable<WeatherID> ids)
        {
            if (ids.Count() != MaxWeatherDataSize)
            {
                throw new ArgumentOutOfRangeException(nameof(ids), MaxWeatherDataSizeErrorMessage);
            }

            if (this.AppServiceConnection == null)
            {
                await this.InitializeAsync();
            }

            var data = ids.Cast<ushort>().SelectMany(BitConverter.GetBytes).ToArray();

            var message = CreateMessage(Asteroid.WeatherIDsCharacteristicUuid, data);

            var errorMessage = await this.AppServiceConnection.SendMessageWithResponseAsync(message);

            return string.IsNullOrWhiteSpace(errorMessage);
        }

        public async Task<bool> SendMinTemperatuesDataAsync(IEnumerable<short> temperatures)
        {
            if (temperatures.Count() != MaxWeatherDataSize)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatures), MaxWeatherDataSizeErrorMessage);
            }

            if (this.AppServiceConnection == null)
            {
                await this.InitializeAsync();
            }

            var data = temperatures.SelectMany(BitConverter.GetBytes).ToArray();

            var message = CreateMessage(Asteroid.WeatherMinTemperaturesCharacteristicUuid, data);

            var errorMessage = await this.AppServiceConnection.SendMessageWithResponseAsync(message);

            return string.IsNullOrWhiteSpace(errorMessage);
        }

        public async Task<bool> SendMaxTemperatuesDataAsync(IEnumerable<short> temperatures)
        {
            if (temperatures.Count() != MaxWeatherDataSize)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatures), MaxWeatherDataSizeErrorMessage);
            }

            if (this.AppServiceConnection == null)
            {
                await this.InitializeAsync();
            }

            var data = temperatures.SelectMany(BitConverter.GetBytes).ToArray();

            var message = CreateMessage(Asteroid.WeatherMaxTemperaturesCharacteristicUuid, data);

            var errorMessage = await this.AppServiceConnection.SendMessageWithResponseAsync(message);

            return string.IsNullOrWhiteSpace(errorMessage);
        }

        #endregion

        #region Private methods

        async Task InitializeAsync()
        {
            this.AppServiceConnection = new AppServiceConnection
            {
                AppServiceName = "org.winsteroid.weather",
                PackageFamilyName = "31050thunderluca.WinSteroid_0.3.0.0_x86__qe6bt2bhxjap2"
            };
            this.AppServiceConnection.ServiceClosed += OnAppServiceConnectionServiceClosed;

            var appServiceConnectionStatus = await this.AppServiceConnection.OpenAsync();
            if (appServiceConnectionStatus != AppServiceConnectionStatus.Success)
            {
                throw new Exception("Operation failed");
            }
        }

        void OnAppServiceConnectionServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            this.AppServiceConnection.ServiceClosed -= OnAppServiceConnectionServiceClosed;
            this.AppServiceConnection = null;
        }

        static Guid GetCharacteristicUuidByWeatherDataType(WeatherDataType weatherDataType)
        {
            switch (weatherDataType)
            {
                case WeatherDataType.City:
                    return Asteroid.WeatherCityCharacteristicUuid;
                case WeatherDataType.IDs:
                    return Asteroid.WeatherIDsCharacteristicUuid;
                case WeatherDataType.MinTemperatures:
                    return Asteroid.WeatherMinTemperaturesCharacteristicUuid;
                case WeatherDataType.MaxTemperatures:
                    return Asteroid.WeatherMaxTemperaturesCharacteristicUuid;
                default:
                    throw new NotSupportedException($"Unsupported {nameof(weatherDataType)}: {weatherDataType}");
            }
        }

        static ValueSet CreateMessage(Guid characteristicUuid, byte[] data)
        {
            return new ValueSet
            {
                { "uuid", characteristicUuid },
                { "data", data }
            };
        }

        #endregion
    }
}
