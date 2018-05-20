using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using WinSteroid.App.Helpers;
using WinSteroid.App.Models;

namespace WinSteroid.App.Services
{
    public class DeviceService
    {
        private BluetoothLEDevice BluetoothDevice = null;
        public DeviceInformation Current = null;
        
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

        public async void RegisterToScreenshotContentService()
        {
            var characteristic = await this.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (characteristic != null)
            {
                var indicateResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (indicateResult == GattCommunicationStatus.Success)
                {
                    characteristic.ValueChanged += OnScreenshotContentCharacteristicValueChanged;
                }
            }
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
                var size = ConvertBytesToInt32(bytes); //BitConverter.ToInt32(bytes, 0);
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

            byte[] decodedBytes = null;

            using (var stream = new MemoryStream(this.TotalData))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                var provider = await decoder.GetPixelDataAsync();
                decodedBytes = provider.DetachPixelData();
            }

            //MANAGE
            var storageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("screenshot.jpg", CreationCollisionOption.GenerateUniqueName);

            await FileIO.WriteBytesAsync(storageFile, decodedBytes); //this.TotalData);

            Debug.WriteLine("Screenshot acquired! File: " + storageFile.Path);

            this.Progress = null;
            this.TotalSize = null;
            this.TotalData = null;
        }

        public async void TakeScreenshotAsync()
        {
            var bytes = new byte[] { 0x0 };

            var result = await this.WriteByteArrayToCharacteristicAsync(Asteroid.ScreenshotRequestCharacteristicUuid, bytes);
            if (!result)
            {
                //ERROR
                throw new Exception();
            }
        }
    }
}