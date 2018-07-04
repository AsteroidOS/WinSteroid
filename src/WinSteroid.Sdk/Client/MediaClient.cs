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
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using WinSteroid.Shared;
using WinSteroid.Shared.Models;

namespace WinSteroid.Sdk.Client
{
    public class MediaClient
    {
        #region Fields

        AppServiceConnection AppServiceConnection;

        static readonly string InProcessMediaCommandBackgroundTaskName = Package.Current.DisplayName.Replace(' ', '_') + "_WinSteroidSdkMediaCommandTask";

        #endregion

        #region Public methods

        public async Task<bool> SendMetaDataUpdateAsync(string data, MediaDataType dataType)
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

            var errorMessage = await this.AppServiceConnection.SendMessageWithResponseAsync(message);

            return string.IsNullOrWhiteSpace(errorMessage);
        }

        public static MediaCommandType? GetMediaCommand(BackgroundActivatedEventArgs args, bool manageDeferral = false)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var deferral = args.TaskInstance.GetDeferral();

            if (!(args.TaskInstance.TriggerDetails is GattCharacteristicNotificationTriggerDetails triggerDetails))
            {
                if (manageDeferral)
                {
                    deferral.Complete();
                }

                return null;
            }

            var bytes = new byte[triggerDetails.Value.Length];
            DataReader.FromBuffer(triggerDetails.Value).ReadBytes(bytes);

            return bytes[0].GetMediaCommandType();
        }

        public static async Task<bool> RegisterInProcessMediaCommandBackgroundTask(GattCharacteristic characteristic)
        {
            if (BackgroundTaskRegistration.AllTasks.Any(kvp => string.Equals(kvp.Value.Name, InProcessMediaCommandBackgroundTaskName))) return true;

            if (characteristic.Uuid != Asteroid.MediaCommandCharacteristicUuid)
            {
                throw new InvalidOperationException($"Invalid GATT Characteristic. Expected {Asteroid.MediaCommandCharacteristicUuid}, received {characteristic.Uuid}");
            }

            var canExecuteBackgroundTasks = await CheckIfApplicationCanExecuteBackgroundTasks();
            if (!canExecuteBackgroundTasks) return false;

            var builder = new BackgroundTaskBuilder { Name = InProcessMediaCommandBackgroundTaskName };
            builder.SetTrigger(new GattCharacteristicNotificationTrigger(characteristic));
            var result = builder.Register();

            return result != null;
        }

        #endregion

        #region Private methods

        async Task InitializeAsync()
        {
            this.AppServiceConnection = new AppServiceConnection
            {
                AppServiceName = "org.winsteroid.media",
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

        static Guid GetCharacteristicUuidByMediaDataType(MediaDataType mediaDataType)
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

        static async Task<bool> CheckIfApplicationCanExecuteBackgroundTasks()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            return backgroundAccessStatus != BackgroundAccessStatus.DeniedByUser
                && backgroundAccessStatus != BackgroundAccessStatus.DeniedBySystemPolicy
                && backgroundAccessStatus != BackgroundAccessStatus.Unspecified;
        }

        #endregion
    }
}