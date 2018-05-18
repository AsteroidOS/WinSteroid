using System;

namespace WinSteroid.App.Models
{
    public class BLECharacteristic
    {
        public BLECharacteristic(string name, Guid uuid)
        {
            this.Name = name;
            this.Uuid = uuid;
        }

        public string Name { get; }

        public Guid Uuid { get; }
    }
}
