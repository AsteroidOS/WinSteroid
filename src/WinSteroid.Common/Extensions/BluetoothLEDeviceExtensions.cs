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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.Shared;

namespace Windows.Devices.Bluetooth
{
    public static class BluetoothLEDeviceExtensions
    {
        public static async Task<GattCharacteristic> GetGattCharacteristicAsync(this BluetoothLEDevice bluetoothLEDevice, Guid characteristicUuid)
        {
            if (bluetoothLEDevice == null)
            {
                throw new ArgumentNullException(nameof(bluetoothLEDevice));
            }

            var service = Asteroid.Services.FirstOrDefault(s => s.Characteristics.Any(c => c.Uuid == characteristicUuid));
            if (service == null) return null;

            try
            {
                var serviceResult = await bluetoothLEDevice.GetGattServicesForUuidAsync(service.Uuid);
                if (serviceResult.Status != GattCommunicationStatus.Success || serviceResult.Services.Count == 0) return null;

                var characteristicResult = await serviceResult.Services[0].GetCharacteristicsForUuidAsync(characteristicUuid);
                if (characteristicResult.Status != GattCommunicationStatus.Success || characteristicResult.Characteristics.Count == 0) return null;

                return characteristicResult.Characteristics[0];
            }
            catch (Exception exception)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(exception);

                return null;
            }
        }
    }
}
