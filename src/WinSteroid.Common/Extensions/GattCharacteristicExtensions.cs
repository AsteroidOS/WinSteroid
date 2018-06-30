using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace Windows.Devices.Bluetooth.GenericAttributeProfile
{
    public static class GattCharacteristicExtensions
    {
        public static async Task<bool> WriteByteArrayToCharacteristicAsync(this GattCharacteristic characteristic, byte[] bytes)
        {
            try
            {
                if (characteristic == null)
                {
                    throw new ArgumentNullException(nameof(characteristic));
                }

                var writeOperationResult = await characteristic.WriteValueAsync(bytes.AsBuffer());

                return writeOperationResult == GattCommunicationStatus.Success;
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);
                return false;
            }
        }
    }
}
