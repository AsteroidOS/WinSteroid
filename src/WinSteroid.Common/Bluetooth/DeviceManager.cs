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

using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Models;

namespace WinSteroid.Common.Bluetooth
{
    public static class DeviceManager
    {
        private static int? TotalSize = null;
        private static byte[] TotalData = null;
        private static int? Progress = null;
        private static List<int> PacketSizes = null;
        private static Stopwatch Stopwatch;

        private static BluetoothLEDevice BluetoothDevice { get; set; }

        private static DeviceInformation Current { get; set; }

        public static string DeviceId => Current?.Id ?? string.Empty;

        public static string DeviceName => Current?.Name ?? string.Empty;

        public static void UpdateLastSavedDeviceInfo(string deviceId, string deviceName)
        {
            SettingsHelper.SetValue(Constants.LastSavedDeviceIdSettingKey, deviceId);
            SettingsHelper.SetValue(Constants.LastSavedDeviceNameSettingKey, deviceName);
        }

        public static IAsyncOperation<DeviceInformation> PickSingleDeviceAsync()
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                throw new ArgumentException(nameof(rootFrame));
            }

            var devicePicker = new DevicePicker();
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));

            return devicePicker.PickSingleDeviceAsync(rootFrame.GetPickerRect());
        }

        public static Task<string> ConnectAsync()
        {
            return ConnectAsync(SettingsHelper.GetValue(Constants.LastSavedDeviceIdSettingKey, string.Empty));
        }

        public static async Task<string> ConnectAsync(string deviceId)
        {
            try
            {
                var bluetoothDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
                if (bluetoothDevice == null)
                {
                    return ResourcesHelper.GetLocalizedString("DeviceServiceDisappearedDeviceError");
                }

                var timeServicesResult = await bluetoothDevice.GetGattServicesForUuidAsync(Asteroid.TimeServiceUuid);
                if (timeServicesResult.Status != GattCommunicationStatus.Success
                    || (timeServicesResult.Status == GattCommunicationStatus.Success && timeServicesResult.Services.Count < 1))
                {
                    return ResourcesHelper.GetLocalizedString("DeviceServiceNoAsteroidDeviceError");
                }

                BluetoothDevice = bluetoothDevice;
                BluetoothDevice.ConnectionStatusChanged += OnBluetoothDeviceConnectionStatusChanged;
                Current = bluetoothDevice.DeviceInformation;
            }
            catch (Exception exception)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(exception);

                return ResourcesHelper.GetLocalizedString("DeviceServiceGenericConnectionError");
            }

            return BluetoothDevice != null && Current != null ? string.Empty : ResourcesHelper.GetLocalizedString("DeviceServiceConnectionFailedError");
        }

        private static void OnBluetoothDeviceConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Messenger.Default.Send(new Messages.DeviceConnectionStatusMessage(sender?.ConnectionStatus == BluetoothConnectionStatus.Connected));
        }

        public static async Task DisconnectAsync()
        {
            var batteryCharacteristic = await BluetoothDevice.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);
            if (batteryCharacteristic != null)
            {
                await batteryCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            }

            var screenshotsCharacteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (screenshotsCharacteristic != null)
            {
                await screenshotsCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            }

            if (BluetoothDevice != null)
            {
                BluetoothDevice.ConnectionStatusChanged -= OnBluetoothDeviceConnectionStatusChanged;
                BluetoothDevice.Dispose();
                BluetoothDevice = null;
                Current = null;

                UpdateLastSavedDeviceInfo(string.Empty, string.Empty);
            }
        }

        public static async Task<PairingResult> PairAsync()
        {
            if (Current.Pairing.IsPaired)
            {
                DeviceManager.UpdateLastSavedDeviceInfo(Current.Id, Current.Name);
                return PairingResult.Success;
            }

            if (!Current.Pairing.CanPair)
            {
                return new PairingResult(ResourcesHelper.GetLocalizedString("DeviceServiceCannotPairError"));
            }

            var pairingResult = await Current.Pairing.PairAsync();
            if (pairingResult.Status == DevicePairingResultStatus.Paired || pairingResult.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                DeviceManager.UpdateLastSavedDeviceInfo(Current.Id, Current.Name);
                return PairingResult.Success;
            }

            if (pairingResult.Status == DevicePairingResultStatus.Failed)
            {
                return PairingResult.PairingRequired;
            }

            return new PairingResult(ResourcesHelper.GetLocalizedString("DeviceServicePairingOperationDeniedOrFailed"));
        }

        public static async Task<bool> InsertNotificationAsync(UserNotification userNotification, ApplicationPreference application)
        {
            if (application != null && application.Muted) return true;

            var xmlNotification = AsteroidHelper.CreateInsertNotificationCommandXml(
                packageName: userNotification.AppInfo.PackageFamilyName,
                id: userNotification.Id.ToString(),
                applicationName: userNotification.AppInfo.DisplayInfo.DisplayName,
                applicationIcon: (application?.Icon ?? default(ApplicationIcon)).GetId(),
                summary: userNotification.GetTitle(),
                body: userNotification.GetBody(),
                vibrationLevel: application?.Vibration ?? VibrationLevel.None);
            
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.NotificationUpdateCharacteristicUuid);

            return await characteristic.WriteByteArrayToCharacteristicAsync(Encoding.UTF8.GetBytes(xmlNotification));
        }

        public static async Task<bool> RemoveNotificationAsync(string notificationId)
        {
            var xmlNotification = AsteroidHelper.CreateRemoveNotificationCommandXml(notificationId);
            
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.NotificationUpdateCharacteristicUuid);

            return await characteristic.WriteByteArrayToCharacteristicAsync(Encoding.UTF8.GetBytes(xmlNotification));
        }

        public static async Task<int> GetBatteryPercentageAsync()
        {
            if (BluetoothDevice == null) return 0;

            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(GattCharacteristicUuids.BatteryLevel);
            if (characteristic == null) return 0;

            var valueResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (valueResult.Status != GattCommunicationStatus.Success) return 0;

            return BatteryHelper.GetPercentage(valueResult?.Value);
        }

        public static async Task<bool> RegisterToScreenshotContentServiceBenchmark()
        {
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (characteristic == null) return false;

            var notifyResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (notifyResult != GattCommunicationStatus.Success) return false;

            characteristic.ValueChanged += OnScreenshotContentBenchmarkCharacteristicValueChanged;
            return true;
        }

        public static async Task<bool> TestScreenshotContentServiceAsync()
        {
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.ScreenshotRequestCharacteristicUuid);
            if (characteristic == null) return false;

            return await characteristic.WriteByteArrayToCharacteristicAsync(new byte[] { 0x0 });
        }

        public static async Task<bool> RegisterToScreenshotContentService()
        {
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (characteristic == null) return false;

            var notifyResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (notifyResult != GattCommunicationStatus.Success) return false;

            characteristic.ValueChanged += OnScreenshotContentCharacteristicValueChanged;
            return true;
        }

        public static async Task<bool> TakeScreenshotAsync()
        {
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.ScreenshotRequestCharacteristicUuid);
            if (characteristic == null) return false;

            return await characteristic.WriteByteArrayToCharacteristicAsync(new byte[] { 0x1 });
        }

        public static Task<bool> SetTimeAsync(DateTime dateTime)
        {
            return SetTimeAsync(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        public static async Task<bool> SetTimeAsync(int year, int month, int day, int hour, int minute, int second)
        {
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.TimeSetCharacteristicUuid);
            if (characteristic == null) return false;

            var yearByte = Convert.ToByte(year - 1900);
            var monthByte = Convert.ToByte(month - 1);
            var dayByte = Convert.ToByte(day);
            var hourByte = Convert.ToByte(hour);
            var minuteByte = Convert.ToByte(minute);
            var secondByte = Convert.ToByte(second);
            
            return await characteristic.WriteByteArrayToCharacteristicAsync(new[] { yearByte, monthByte, dayByte, hourByte, minuteByte, secondByte });
        }

        private static void OnBatteryLevelValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            Messenger.Default.Send(new Messages.DeviceBatteryMessage(BatteryHelper.GetPercentage(args.CharacteristicValue)));
        }

        public static async Task<bool> UnregisterToScreenshotContentService()
        {
            var characteristic = await BluetoothDevice.GetGattCharacteristicAsync(Asteroid.ScreenshotContentCharacteristicUuid);
            if (characteristic == null) return true;

            try
            {
                var notifyResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (notifyResult != GattCommunicationStatus.Success) return false;

                TotalSize = null;
                TotalData = null;
                Progress = null;
                Messenger.Default.Send(new Messages.ScreenshotProgressMessage(percentage: 0));

                return true;
            }
            catch (Exception exception)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(exception);

                return false;
            }
        }

        private static void OnScreenshotContentBenchmarkCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var bytes = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bytes);

            if (!TotalSize.HasValue)
            {
                try
                {
                    var size = BitConverter.ToInt32(bytes, 0);
                    TotalData = new byte[size];
                    TotalSize = size;
                    Progress = 0;
                    PacketSizes = new List<int>();
                    Stopwatch = new Stopwatch();
                    Stopwatch.Start();
                }
                catch (ArithmeticException)
                {
                    TotalSize = null;
                    TotalData = null;
                    Progress = null;
                    PacketSizes = null;
                    if (Stopwatch?.IsRunning ?? false)
                    {
                        Stopwatch.Stop();
                    }
                    Stopwatch = null;

                    sender.ValueChanged -= OnScreenshotContentBenchmarkCharacteristicValueChanged;
                }

                return;
            }

            PacketSizes.Add(bytes.Length);

            if (PacketSizes.Count < 100) return;

            Stopwatch.Stop();

            sender.ValueChanged -= OnScreenshotContentBenchmarkCharacteristicValueChanged;

            var message = new Messages.ScreenshotBenchmarkMessage((int)PacketSizes.Average(), Stopwatch.ElapsedMilliseconds);

            Messenger.Default.Send(message);

            TotalSize = null;
            TotalData = null;
            Progress = null;
            PacketSizes = null;
            Stopwatch = null;
        }

        private static async void OnScreenshotContentCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var bytes = new byte[args.CharacteristicValue.Length];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(bytes);

            if (!TotalSize.HasValue)
            {
                var size = BitConverter.ToInt32(bytes, 0);
                TotalData = new byte[size];
                TotalSize = size;
                Progress = 0;
                return;
            }

            if (Progress.Value + bytes.Length <= TotalData.Length)
            {
                Array.Copy(bytes, 0, TotalData, Progress.Value, bytes.Length);
            }

            Progress += bytes.Length;

            if (Progress.Value < TotalSize.Value)
            {
                var percentage = (int)Math.Ceiling((Progress.Value * 100d) / TotalSize.Value);
                Messenger.Default.Send(new Messages.ScreenshotProgressMessage(percentage));
                return;
            }

            var screenshotsFolder = await FilesHelper.GetScreenshotsFolderAsync();

            var fileName = $"{Package.Current.DisplayName}_Screenshot_{DateTime.Now.ToString("yyyyMMdd")}_{DateTime.Now.ToString("HHmmss")}.jpg";

            var screenshotFile = await screenshotsFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(screenshotFile, TotalData);

            ToastsHelper.Show(string.Format(ResourcesHelper.GetLocalizedString("ScreenshotAcquiredMessageFormat"), fileName));

            sender.ValueChanged -= OnScreenshotContentCharacteristicValueChanged;

            TotalSize = null;
            TotalData = null;
            Progress = null;

            Messenger.Default.Send(new Messages.ScreenshotAcquiredMessage(fileName));
        }
    }
}
