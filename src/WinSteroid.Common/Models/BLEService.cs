using System;
using System.Collections.Generic;
using System.Linq;

namespace WinSteroid.Common.Models
{
    public class BLEService
    {
        public BLEService(string name, Guid uuid, IEnumerable<BLECharacteristic> characteristics)
        {
            this.Name = name;
            this.Uuid = uuid;
            this.Characteristics = characteristics.ToList();
        }

        public string Name { get; }

        public Guid Uuid { get; }

        public IList<BLECharacteristic> Characteristics { get; }
    }
}
