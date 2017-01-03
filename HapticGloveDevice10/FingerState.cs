using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;

namespace HapticGlove
{
    public class FingerState
    {
        private static Regex indexPattern = new Regex("^Sensor (\\d+)$");
        private List<float> values;
        private List<GattCharacteristic> sensors;

        public FingerState()
        {
            this.values = new List<float>();
            this.sensors = new List<GattCharacteristic>();
        }

        public bool HasFinger(int index)
        {
            return 0 <= index && index < this.sensors.Count && this.sensors[index] != null;
        }

        public int Count
        {
            get
            {
                return Math.Min(this.sensors.Count, this.values.Count);
            }
        }

        public float this[int index]
        {
            get
            {
                if(0 <= index && index < this.values.Count)
                {
                    return this.values[index];
                }
                return 0f;
            }
        }

        public bool Ready
        {
            get
            {
                return this.Count >= 5;
            }
        }

        private static int GetIndex(string description)
        {
            var match = indexPattern.Match(description);
            if(match != null && match.Groups.Count > 1)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return -1;
        }

        public async Task Connect(string description, GattCharacteristic sensor)
        {
            if(sensor.Uuid == GATTDefaultCharacteristic.Analog.UUID && sensor.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            {
                var index = GetIndex(description);
                if(0 <= index)
                {
                    while(index >= this.sensors.Count)
                    {
                        this.sensors.Add(null);
                        this.values.Add(0f);
                    }
                    if(this.sensors[index] == null)
                    {
                        this.sensors[index] = sensor;
                        this.values[index] = await Glove.GetValue(sensor) / 256f;

                        await sensor.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        sensor.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
                        {
                            this.values[index] = Glove.GetByte(args.CharacteristicValue) / 256f;
                        };
                    }
                }
            }
        }
    }
}
