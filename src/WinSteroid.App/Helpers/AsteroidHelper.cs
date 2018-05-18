using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using WinSteroid.App.Models;

namespace WinSteroid.App.Helpers
{
    public static class AsteroidHelper
    {
        public static async Task CheckExistingServicesAndCharacteristics(BluetoothLEDevice bluetoothDevice)
        {
            var servicesResult = await bluetoothDevice.GetGattServicesAsync();
            if (servicesResult.Status == GattCommunicationStatus.Success)
            {
                Debug.WriteLine($"Found {servicesResult.Services?.Count ?? 0} services");
                foreach (var service in servicesResult.Services)
                {
                    Debug.WriteLine($"Found service with uuid {service.Uuid}.");
                    var knownService = Asteroid.Services.SingleOrDefault(s => s.Uuid == service.Uuid);
                    if (knownService != null)
                    {
                        Debug.WriteLine($"Service '{knownService.Name}' found! :D");
                        var characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status == GattCommunicationStatus.Success)
                        {
                            Debug.WriteLine($"Found {characteristicsResult.Characteristics?.Count ?? 0} characteristics");
                            foreach (var characteristic in characteristicsResult.Characteristics)
                            {
                                Debug.WriteLine($"Found characteristic with uuid {characteristic.Uuid}.");
                                var knownCharacteristic = knownService.Characteristics.SingleOrDefault(c => c.Uuid == characteristic.Uuid);
                                if (knownCharacteristic != null)
                                {
                                    Debug.WriteLine($"Characteristic '{knownCharacteristic.Name}' found! :D :D");
                                }
                                else
                                {
                                    Debug.WriteLine($"Characteristic not found! :C :C Uuid: '{characteristic.Uuid}', name: '{characteristic.UserDescription}'");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Service not found! :C Uuid: '{service.Uuid}'");
                    }
                }
            }
        }

        public static string CreateInsertNotificationCommandXml(string packageName, string id, string applicationName, string applicationIcon, string summary, string body, VibrationLevel vibrationLevel = VibrationLevel.None)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("<insert>");
            stringBuilder.AppendNode("pn", packageName);
            stringBuilder.AppendNode("id", id);
            stringBuilder.AppendNode("an", applicationName);
            stringBuilder.AppendNode("ai", applicationIcon);
            stringBuilder.AppendNode("su", summary);
            stringBuilder.AppendNode("bo", body);
            stringBuilder.AppendNode("vb", vibrationLevel.GetDisplayName());
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
