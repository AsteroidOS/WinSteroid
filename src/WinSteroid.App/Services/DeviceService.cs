﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using WinSteroid.App.Helpers;
using WinSteroid.App.Models;

namespace WinSteroid.App.Services
{
    public class DeviceService
    {
        private DeviceWatcher Watcher;
        private BluetoothLEDevice BluetoothDevice;
        
        private readonly string[] RequestedProperties = new[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
        public readonly List<DeviceInformation> Devices;

        public DeviceService()
        {
            this.Devices = new List<DeviceInformation>();
        }

        public void StartSearch()
        {
            this.Devices.Clear();

            this.Watcher = DeviceInformation.CreateWatcher(
                BluetoothLEDevice.GetDeviceSelectorFromPairingState(pairingState: true), 
                this.RequestedProperties, 
                DeviceInformationKind.AssociationEndpoint);
            
            this.Watcher.Updated += OnDeviceUpdated;
            this.Watcher.Added += OnDeviceAdded;
            this.Watcher.Removed += OnDeviceRemoved;

            this.Watcher.Start();
        }

        public void StopSearch()
        {
            if (this.Watcher.Status != DeviceWatcherStatus.Started) return;

            this.Watcher.Stop();
        }

        public bool IsSearching() => this.Watcher.Status == DeviceWatcherStatus.Started;

        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation device)
        {
            if (string.IsNullOrWhiteSpace(device?.Name)) return;

            if (this.Devices.Any(d => d.Id == device.Id)) return;

            this.Devices.Add(device);
        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate updatedDevice)
        {
            var device = this.Devices.FirstOrDefault(d => d.Id == updatedDevice.Id);
            if (device == null) return;

            device.Update(updatedDevice);
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate removedDevice)
        {
            if (this.Devices.All(d => d.Id != removedDevice.Id)) return;

            this.Devices.Where(d => d.Id != removedDevice.Id).ToList();
        }
        
        public async Task<bool> ConnectAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return false;

            try
            {
                this.BluetoothDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            }
            catch
            {
                //ERROR
                return false;
            }

            return this.BluetoothDevice != null;
        }

        public async Task<PairingResult> PairAsync(string deviceId)
        {
            var device = this.Devices.SingleOrDefault(d => d.Id == deviceId);
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (device.Pairing.IsPaired)
            {
                SettingsHelper.SetValue("lastSavedDeviceId", deviceId);
                return PairingResult.Success;
            }

            if (!device.Pairing.CanPair)
            {
                return new PairingResult("The selected device cannot be paired");
            }

            var pairingResult = await device.Pairing.PairAsync();
            if (pairingResult.Status == DevicePairingResultStatus.Paired || pairingResult.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                SettingsHelper.SetValue("LastSavedDeviceId", deviceId);
                return PairingResult.Success;
            }

            return new PairingResult("Pairing operation denied or failed");
        }

        public void Disconnect()
        {
            if (this.BluetoothDevice != null)
            {
                this.BluetoothDevice.Dispose();
                this.BluetoothDevice = null;
            }
        }

        public async Task<GattCharacteristic> GetGattCharacteristicAsync(Guid characteristicUuid)
        {
            if (this.BluetoothDevice == null)
            {
                var lastSavedDeviceId = SettingsHelper.GetValue("lastSavedDeviceId", string.Empty);
                await this.ConnectAsync(lastSavedDeviceId);
            }

            if (this.BluetoothDevice == null) //Again
            {
                //ERRROR
                throw new Exception();
            }

            var service = Asteroid.Services.FirstOrDefault(s => s.Characteristics.Any(c => c.Uuid == characteristicUuid));
            if (service == null)
            {
                //ERRROR
                throw new Exception();
            }

            var serviceResult = await this.BluetoothDevice.GetGattServicesForUuidAsync(service.Uuid);
            if (serviceResult.Status != GattCommunicationStatus.Success || serviceResult.Services.Count == 0)
            {
                //ERROR
                throw new Exception();
            }

            var characteristicResult = await serviceResult.Services[0].GetCharacteristicsForUuidAsync(characteristicUuid);
            if (characteristicResult.Status != GattCommunicationStatus.Success || characteristicResult.Characteristics.Count == 0)
            {
                //ERROR
                throw new Exception();
            }

            return characteristicResult.Characteristics[0];
        }

        private async Task<bool> WriteSingleByteToCharacteristicAsync(Guid characteristicUuid, byte @byte)
        {
            var characteristic = await this.GetGattCharacteristicAsync(characteristicUuid);

            var writer = new DataWriter();
            writer.WriteByte(@byte);

            var writeOperationResult = await characteristic.WriteValueAsync(writer.DetachBuffer());
            writer.Dispose();

            return writeOperationResult == GattCommunicationStatus.Success;
        }

        private async Task<bool> WriteByteArrayToCharacteristicAsync(Guid characteristicUuid, byte[] bytes)
        {
            var characteristic = await this.GetGattCharacteristicAsync(characteristicUuid);

            var writer = new DataWriter();
            writer.WriteBytes(bytes);

            var writeOperationResult = await characteristic.WriteValueAsync(writer.DetachBuffer());
            writer.Dispose();

            return writeOperationResult == GattCommunicationStatus.Success;
        }

        public async Task<ushort> GetBatteryPercentageAsync()
        {
            if (this.BluetoothDevice == null) return 0;

            var characteristic = await this.GetGattCharacteristicAsync(Asteroid.BatteryLevelCharacteristicUuid);

            var valueResult = await characteristic.ReadValueAsync();
            if (valueResult.Status != GattCommunicationStatus.Success)
            {
                //ERROR
                throw new Exception();
            }

            using (var reader = DataReader.FromBuffer(valueResult.Value))
            {
                var bytes = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(bytes);

                return Convert.ToUInt16(bytes[0]);
            }
        }

        public Task<bool> SendMediaCommandAsync(MediaCommandType mediaCommand)
        {
            var commandByte = (byte)mediaCommand;
            
            return this.WriteSingleByteToCharacteristicAsync(Asteroid.CommandCharacteristicUuid, commandByte);
        }

        public Task<bool> InsertNotificationAsync(UserNotification userNotification)
        {
            var xmlNotification = AsteroidHelper.CreateInsertNotificationCommandXml(
                packageName: userNotification.AppInfo.PackageFamilyName,
                id: userNotification.Id.ToString(),
                applicationName: userNotification.AppInfo.DisplayInfo.DisplayName,
                applicationIcon: "ios-alert", //TODO
                summary: userNotification.GetTitle(),
                body: userNotification.GetBody());

            var utf8Bytes = Encoding.UTF8.GetBytes(xmlNotification);

            return this.WriteByteArrayToCharacteristicAsync(Asteroid.NotificationUpdateCharacteristicUuid, utf8Bytes);
        }

        public Task<bool> RemoveNotificationAsync(string notificationId)
        {
            var xmlNotification = AsteroidHelper.CreateRemoveNotificationCommandXml(notificationId);

            var utf8Bytes = Encoding.UTF8.GetBytes(xmlNotification);

            return this.WriteByteArrayToCharacteristicAsync(Asteroid.NotificationUpdateCharacteristicUuid, utf8Bytes);
        }
    }
}