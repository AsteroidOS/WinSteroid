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
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using WinSteroid.Shared;
using WinSteroid.Shared.Models;

namespace WinSteroid.Sdk.Client
{
    public class MediaClient
    {
        private AppServiceConnection AppServiceConnection;

        private async Task InitializeAsync()
        {
            this.AppServiceConnection = new AppServiceConnection
            {
                AppServiceName = "org.winsteroid.media",
                PackageFamilyName = "31050thunderluca.WinSteroid_0.3.0.0_x86__qe6bt2bhxjap2"
            };

            var appServiceConnectionStatus = await this.AppServiceConnection.OpenAsync();
            if (appServiceConnectionStatus != AppServiceConnectionStatus.Success)
            {
                throw new Exception("Operation failed");
            }
        }

        public async Task<bool> SendDataAsync(string data, MediaDataType dataType)
        {
            if (this.AppServiceConnection == null)
            {
                await this.InitializeAsync();
            }

            var message = new ValueSet
            {
                { "uuid", GetCharacteristicUuidByMediaDataType(dataType) },
                { "data", data }
            };

            var appServiceResponse = await this.AppServiceConnection.SendMessageAsync(message);
            if (appServiceResponse.Status != AppServiceResponseStatus.Success)
            {
                throw new Exception("Operation failed");
            }

            var responseMessage = appServiceResponse.Message;
            if (!responseMessage.ContainsKey("success"))
            {
                throw new Exception("Fatal error");
            }

            var success = (bool)responseMessage["success"];
            if (!success)
            {
                var errorMessage = responseMessage.ContainsKey("errorMessage") ? responseMessage["errorMessage"] as string : "Unknown reason";

                throw new Exception(errorMessage);
            }

            return true;
        }

        private static Guid GetCharacteristicUuidByMediaDataType(MediaDataType mediaDataType)
        {
            switch (mediaDataType)
            {
                case MediaDataType.Title:
                    return Asteroid.MediaTitleCharacteristicUuid;
                case MediaDataType.Album:
                    return Asteroid.MediaAlbumCharacteristicUuid;
                case MediaDataType.Artist:
                    return Asteroid.MediaArtistCharacteristicUuid;
                case MediaDataType.PlayerStatus:
                    return Asteroid.MediaPlayingCharacteristicUuid;
                default:
                    throw new NotSupportedException($"Unsupported {nameof(MediaDataType)}: {mediaDataType}");
            }
        }
    }
}
