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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using WinSteroid.Common;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Services
{
    public class DeviceService
    {
        private readonly ApplicationsService ApplicationsService;
        private readonly List<GattCharacteristic> CachedCharacteristics;

        public DeviceService(ApplicationsService applicationsService)
        {
            this.ApplicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
            this.CachedCharacteristics = new List<GattCharacteristic>();
        }

        public BluetoothLEDevice BluetoothDevice { get; private set; }

        private TypedEventHandler<BluetoothLEDevice, object> ConnectionStatusChangedEventHandler { get; set; }

        public DeviceInformation Current { get; private set; }

        public string GetLastSavedDeviceId() => SettingsHelper.GetValue(Constants.LastSavedDeviceIdSettingKey, string.Empty);

        public string GetLastSavedDeviceName() => SettingsHelper.GetValue(Constants.LastSavedDeviceNameSettingKey, string.Empty);

        public void UpdateLastSavedDeviceInfo()
        {
            SettingsHelper.SetValue(Constants.LastSavedDeviceIdSettingKey, this.Current?.Id ?? string.Empty);
            SettingsHelper.SetValue(Constants.LastSavedDeviceNameSettingKey, this.Current?.Name ?? string.Empty);
        }

        public async Task<string> ConnectAsync(string deviceId)
        {
            try
            {
                var bluetoothDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
                if (bluetoothDevice == null)
                {
                    //DISAPPEARED DEVICE
                    return "I cannot connect to the selected Bluetooth device, seems to be disappeared";
                }
                
                var timeServicesResult = await bluetoothDevice.GetGattServicesForUuidAsync(Asteroid.TimeServiceUuid);
                if (timeServicesResult.Status != GattCommunicationStatus.Success 
                    || (timeServicesResult.Status == GattCommunicationStatus.Success && timeServicesResult.Services.Count < 1))
                {
                    //WRONG DEVICE
                    return "I cannot connect to the selected Bluetooth device, seems to not be an AsteroidOS device";
                }

                this.BluetoothDevice = bluetoothDevice;
                this.Current = bluetoothDevice.DeviceInformation;
            }
            catch (Exception exception)
            {
                //ERROR
#if DEBUG
                return "An error occured. Exception: " + exception.ToString();
#else
                return "An error occured during connection";
#endif
            }

            return this.BluetoothDevice != null && this.Current != null ? string.Empty : "Connection failed";
        }

        public void AttachConnectionStatusChangedHandler(TypedEventHandler<BluetoothLEDevice, object> connectionStatusChangedHandler)
        {
            this.ConnectionStatusChangedEventHandler = connectionStatusChangedHandler;
            this.BluetoothDevice.ConnectionStatusChanged += this.ConnectionStatusChangedEventHandler;
        }

        public async Task<PairingResult> PairAsync()
        {
            if (this.Current.Pairing.IsPaired)
            {
                this.UpdateLastSavedDeviceInfo();
                return PairingResult.Success;
            }

            if (!this.Current.Pairing.CanPair)
            {
                return new PairingResult("The selected device cannot be paired");
            }

            var pairingResult = await this.Current.Pairing.PairAsync();
            if (pairingResult.Status == DevicePairingResultStatus.Paired || pairingResult.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                this.UpdateLastSavedDeviceInfo();
                return PairingResult.Success;
            }

            return new PairingResult("Pairing operation denied or failed");
        }

        public async Task DisconnectAsync()
        {
            if (!this.CachedCharacteristics.IsNullOrEmpty())
            {
                foreach (var characteristic in this.CachedCharacteristics)
                {
                    await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    this.CachedCharacteristics.Remove(characteristic);
                }
            }

            if (this.BluetoothDevice != null)
            {
                if (this.ConnectionStatusChangedEventHandler != null)
                {
                    this.BluetoothDevice.ConnectionStatusChanged -= ConnectionStatusChangedEventHandler;
                    this.ConnectionStatusChangedEventHandler = null;
                }

                this.BluetoothDevice.Dispose();
                this.BluetoothDevice = null;
                this.Current = null;

                this.UpdateLastSavedDeviceInfo();
            }
        }

        public async Task<GattCharacteristic> GetGattCharacteristicAsync(Guid characteristicUuid)
        {
            var cachedCharacteristic = this.CachedCharacteristics.FirstOrDefault(gc => Equals(gc.Uuid, characteristicUuid));
            if (cachedCharacteristic != null) return cachedCharacteristic;

            var service = Asteroid.Services.FirstOrDefault(s => s.Characteristics.Any(c => c.Uuid == characteristicUuid));
            if (service == null) return null;

            var serviceResult = await this.BluetoothDevice.GetGattServicesForUuidAsync(service.Uuid);
            if (serviceResult.Status != GattCommunicationStatus.Success || serviceResult.Services.Count == 0) return null;

            var characteristicResult = await serviceResult.Services[0].GetCharacteristicsForUuidAsync(characteristicUuid);
            if (characteristicResult.Status != GattCommunicationStatus.Success || characteristicResult.Characteristics.Count == 0) return null;

            var characteristic = characteristicResult.Characteristics[0];
            if (this.CachedCharacteristics.All(gc => !Equals(characteristic.Uuid, gc.Uuid)))
            {
                this.CachedCharacteristics.Add(characteristic);
            }

            return characteristic;
        }

        private async Task<bool> WriteByteArrayToCharacteristicAsync(Guid characteristicUuid, byte[] bytes)
        {
            var characteristic = await this.GetGattCharacteristicAsync(characteristicUuid);
            if (characteristic == null) return false;
            
            var writeOperationResult = await characteristic.WriteValueAsync(bytes.AsBuffer());

            return writeOperationResult == GattCommunicationStatus.Success;
        }

        public async Task<int> GetBatteryPercentageAsync()
        {
            if (this.BluetoothDevice == null) return 0;

            var characteristic = await this.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);
            if (characteristic == null) return 0;

            var valueResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (valueResult.Status != GattCommunicationStatus.Success) return 0;

            return BatteryHelper.GetPercentage(valueResult.Value);
        }

        public Task<bool> SendMediaCommandAsync(MediaCommandType mediaCommand)
        {
            var commandByte = (byte)mediaCommand;
            
            return this.WriteByteArrayToCharacteristicAsync(Asteroid.CommandCharacteristicUuid, new[] { commandByte });
        }

        public Task<bool> InsertNotificationAsync(UserNotification userNotification)
        {
            var application = this.ApplicationsService.GetApplicationPreferenceByAppId(userNotification.AppInfo.PackageFamilyName);
            if (application != null && application.Muted) return Task.FromResult(true);

            var xmlNotification = AsteroidHelper.CreateInsertNotificationCommandXml(
                packageName: userNotification.AppInfo.PackageFamilyName,
                id: userNotification.Id.ToString(),
                applicationName: userNotification.AppInfo.DisplayInfo.DisplayName,
                applicationIcon: (application?.Icon ?? this.ApplicationsService.GetDefaultApplicationIcon()).GetId(),
                summary: userNotification.GetTitle(),
                body: userNotification.GetBody(),
                vibrationLevel: application?.Vibration ?? VibrationLevel.None);

            var utf8Bytes = Encoding.UTF8.GetBytes(xmlNotification);

            return this.WriteByteArrayToCharacteristicAsync(Asteroid.NotificationUpdateCharacteristicUuid, utf8Bytes);
        }

        public Task<bool> RemoveNotificationAsync(string notificationId)
        {
            var xmlNotification = AsteroidHelper.CreateRemoveNotificationCommandXml(notificationId);

            var utf8Bytes = Encoding.UTF8.GetBytes(xmlNotification);

            return this.WriteByteArrayToCharacteristicAsync(Asteroid.NotificationUpdateCharacteristicUuid, utf8Bytes);
        }

        public IAsyncOperation<DeviceInformation> PickSingleDeviceAsync(FrameworkElement element)
        {
            var devicePicker = new DevicePicker();
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));

            var rect = element.GetPickerRect();

            return devicePicker.PickSingleDeviceAsync(rect);
        }
        
        public async Task<bool> RegisterToScreenshotContentService()
        {
            var characteristic = await this.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (characteristic == null) return false;

            var notifyResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (notifyResult != GattCommunicationStatus.Success) return false;

            characteristic.ValueChanged += OnScreenshotContentCharacteristicValueChanged;
            return true;
        }

        public Task<bool> TakeScreenshotAsync()
        {
            return this.WriteByteArrayToCharacteristicAsync(Asteroid.ScreenshotRequestCharacteristicUuid, new byte[] { 0x1 });
        }

        private int? TotalSize = null;
        private byte[] TotalData = null;
        private int? Progress = null;

        private async void OnScreenshotContentCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var bytes = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bytes);

            if (!this.TotalSize.HasValue)
            {
                var size = BitConverter.ToInt32(bytes, 0);
                this.TotalData = new byte[size];
                this.TotalSize = size;
                this.Progress = 0;
                return;
            }
            
            if (this.Progress.Value + bytes.Length <= this.TotalData.Length)
            {
                Array.Copy(bytes, 0, this.TotalData, this.Progress.Value, bytes.Length);
            }

            this.Progress += bytes.Length;

            System.Diagnostics.Debug.WriteLine("Progress: " + this.Progress.Value + "; Total size: " + this.TotalSize.Value);

            if (this.Progress.Value < this.TotalSize.Value) return;
            
            var storageFilePath = await FilesHelper.WriteBytesAsync("screenshot.jpg", Windows.Storage.KnownFolders.PicturesLibrary, this.TotalData);
            
            ToastsHelper.Show("Screenshot acquired! File: " + storageFilePath);
            
            this.TotalSize = null;
            this.TotalData = null;
            this.Progress = null;
        }

        public Task<bool> SetTimeAsync(DateTime dateTime)
        {
            return this.SetTimeAsync(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        public Task<bool> SetTimeAsync(int year, int month, int day, int hour, int minute, int second)
        {
            var yearByte = Convert.ToByte(year - 1900);
            var monthByte = Convert.ToByte(month - 1);
            var dayByte = Convert.ToByte(day);
            var hourByte = Convert.ToByte(hour);
            var minuteByte = Convert.ToByte(minute);
            var secondByte = Convert.ToByte(second);

            var bytes = new[] { yearByte, monthByte, dayByte, hourByte, minuteByte, secondByte };

            return this.WriteByteArrayToCharacteristicAsync(Asteroid.TimeSetCharacteristicUuid, bytes);
        }
    }
}