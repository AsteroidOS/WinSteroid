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

using System.Text;
using WinSteroid.Common.Models;

namespace WinSteroid.Common.Helpers
{
    public static class AsteroidHelper
    {
        //public static async Task CheckExistingServicesAndCharacteristics(BluetoothLEDevice bluetoothDevice)
        //{
        //    var servicesResult = await bluetoothDevice.GetGattServicesAsync();
        //    if (servicesResult.Status == GattCommunicationStatus.Success)
        //    {
        //        Debug.WriteLine($"Found {servicesResult.Services?.Count ?? 0} services");
        //        foreach (var service in servicesResult.Services)
        //        {
        //            Debug.WriteLine($"Found service with uuid {service.Uuid}.");
        //            var knownService = Asteroid.Services.SingleOrDefault(s => s.Uuid == service.Uuid);
        //            if (knownService != null)
        //            {
        //                Debug.WriteLine($"Service '{knownService.Name}' found! :D");
        //                var characteristicsResult = await service.GetCharacteristicsAsync();
        //                if (characteristicsResult.Status == GattCommunicationStatus.Success)
        //                {
        //                    Debug.WriteLine($"Found {characteristicsResult.Characteristics?.Count ?? 0} characteristics");
        //                    foreach (var characteristic in characteristicsResult.Characteristics)
        //                    {
        //                        Debug.WriteLine($"Found characteristic with uuid {characteristic.Uuid}.");
        //                        var knownCharacteristic = knownService.Characteristics.SingleOrDefault(c => c.Uuid == characteristic.Uuid);
        //                        if (knownCharacteristic != null)
        //                        {
        //                            Debug.WriteLine($"Characteristic '{knownCharacteristic.Name}' found! :D :D");
        //                        }
        //                        else
        //                        {
        //                            Debug.WriteLine($"Characteristic not found! :C :C Uuid: '{characteristic.Uuid}', name: '{characteristic.UserDescription}'");
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Debug.WriteLine($"Service not found! :C Uuid: '{service.Uuid}'");
        //            }
        //        }
        //    }
        //}

        public static string CreateInsertNotificationCommandXml(
            string packageName, 
            string id, 
            string applicationName, 
            string applicationIcon, 
            string summary, 
            string body, 
            VibrationLevel vibrationLevel)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("<insert>");
            stringBuilder.AppendNode("pn", packageName);
            stringBuilder.AppendNode("id", id);
            stringBuilder.AppendNode("an", applicationName);
            stringBuilder.AppendNode("ai", applicationIcon);
            stringBuilder.AppendNode("su", summary);
            stringBuilder.AppendNode("bo", body);
            stringBuilder.AppendNode("vb", vibrationLevel.GetId());
            stringBuilder.Append("</insert>");

            return stringBuilder.ToString();
        }

        public static string CreateRemoveNotificationCommandXml(string id)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("<remove>");
            stringBuilder.AppendNode("id", id);
            stringBuilder.Append("</remove>");

            return stringBuilder.ToString();
        }
    }
}
