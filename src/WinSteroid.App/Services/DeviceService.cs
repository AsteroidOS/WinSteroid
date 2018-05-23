using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage;
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

        public DeviceService(ApplicationsService applicationsService)
        {
            this.ApplicationsService = applicationsService ?? throw new ArgumentNullException(nameof(applicationsService));
        }

        private BluetoothLEDevice BluetoothDevice = null;
        public DeviceInformation Current = null;
        public int BatteryLevel { get; set; }
        public bool IsDeviceConnected { get; set; }

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

                this.BluetoothDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
                this.Current = BluetoothDevice.DeviceInformation;
            }
            catch
            {
                //ERROR
                return false;
            }

            return this.BluetoothDevice != null && this.Current != null;
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            this.IsDeviceConnected = sender.ConnectionStatus == BluetoothConnectionStatus.Connected;
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

        public void Disconnect()
        {
            if (this.BluetoothDevice != null)
            {
                this.BluetoothDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
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

        private static int ConvertBytesToInt32(byte[] bytes)
        {
            int result = 0;
            for (int i = 3; i >= 0; i--)
            {
                result <<= 8;
                result |= (bytes[i] & 0xFF);
            }
            return result;
        }

        private async void OnScreenshotContentCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var dataReader = DataReader.FromBuffer(args.CharacteristicValue);

            var bytes = new byte[dataReader.UnconsumedBufferLength];

            dataReader.ReadBytes(bytes);

            if (!this.TotalSize.HasValue)
            {
                var size = ConvertBytesToInt32(bytes);
                this.TotalData = new byte[size];
                this.TotalSize = size;
                this.Progress = 0;
                return;
            }

            if (bytes.Length + this.Progress.Value <= this.TotalData.Length)
            {
                Array.Copy(bytes, 0, this.TotalData, this.Progress.Value, bytes.Length);
            }

            this.Progress += bytes.Length;
            Debug.WriteLine("Progress: " + this.Progress.Value + "; Total size: " + this.TotalSize.Value);
            if ((this.Progress.Value) < this.TotalSize.Value / 10)
            {
                return;
            }

            //MANAGE
            var storageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("screenshot.jpg", CreationCollisionOption.GenerateUniqueName);

            await FileIO.WriteBytesAsync(storageFile, this.TotalData);

            Debug.WriteLine("Screenshot acquired! File: " + storageFile.Path);

            this.Progress = null;
            this.TotalSize = null;
            this.TotalData = null;
        }
    }
}