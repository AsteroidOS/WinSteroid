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

        public string GetLastSavedDeviceId() => SettingsHelper.GetValue("lastSavedDeviceId", string.Empty);

        public string GetLastSavedDeviceName() => SettingsHelper.GetValue("lastSavedDeviceName", string.Empty);

        public void UpdateLastSavedDeviceInfo()
        {
            SettingsHelper.SetValue("lastSavedDeviceId", this.Current?.Id ?? string.Empty);
            SettingsHelper.SetValue("lastSavedDeviceName", this.Current?.Name ?? string.Empty);
        }

        public async Task<bool> ConnectAsync(string deviceId)
        {
            try
            {
                this.BluetoothDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
                if (this.BluetoothDevice == null) return false;

                this.Current = BluetoothDevice.DeviceInformation;
            }
            catch
            {
                //ERROR
                return false;
            }

            return this.BluetoothDevice != null && this.Current != null;
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
            }

            if (this.Current != null)
            {
                this.Current = null;
                this.UpdateLastSavedDeviceInfo();
            }
        }

        public async Task<GattCharacteristic> GetGattCharacteristicAsync(Guid characteristicUuid)
        {
            var cachedCharacteristic = this.CachedCharacteristics.FirstOrDefault(gc => Equals(gc.Uuid, characteristicUuid));
            if (cachedCharacteristic != null) return cachedCharacteristic;

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
            
            var writeOperationResult = await characteristic.WriteValueAsync(bytes.AsBuffer());

            return writeOperationResult == GattCommunicationStatus.Success;
        }

        public async Task<int> GetBatteryPercentageAsync()
        {
            if (this.BluetoothDevice == null) return 0;

            var characteristic = await this.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);
            
            var valueResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (valueResult.Status != GattCommunicationStatus.Success)
            {
                //ERROR
                throw new Exception();
            }

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

        public async Task<bool> PickSingleDeviceAsync(FrameworkElement element)
        {
            var devicePicker = new DevicePicker();
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));

            var rect = element.GetPickerRect();

            var device = await devicePicker.PickSingleDeviceAsync(rect);
            if (device == null) return false;

            this.Current = device;

            return true;
        }
        
        public async Task<bool> RegisterToScreenshotContentService()
        {
            var characteristic = await this.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (characteristic != null)
            {        
                var notifyResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (notifyResult == GattCommunicationStatus.Success)
                {
                    characteristic.ValueChanged += OnScreenshotContentCharacteristicValueChanged;
                    return true;
                }
            }

            return false;
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
    }
}