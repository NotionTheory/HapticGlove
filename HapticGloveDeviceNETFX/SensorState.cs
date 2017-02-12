using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HapticGlove
{
    public class SensorState
    {
        static List<string> names = new List<string>(new [] {
            "Thumb",
            "Index",
            "Middle",
            "Ring",
            "Pinkie",
            "Battery"
        });

        public ObservableCollection<Sensor> Readers
        {
            get; private set;
        }

        const byte MIN_BATTERY = 125;
        const byte MAX_BATTERY = 167;
        private MotorState motorState;

        public SensorState(MotorState motorState)
        {
            this.motorState = motorState;
            this.Readers = new ObservableCollection<Sensor>();
            for (int i = 0; i < names.Count; ++i)
            {
                this.Readers.Add(new Sensor(names[i], 0, (byte)i, motorState));
            }
        }

        public bool HasSensor(int index)
        {
            return 0 <= index && index < this.Readers.Count && this.Readers[index] != null;
        }

        public int Count
        {
            get
            {
                return this.Readers.Count;
            }
        }

        public Sensor this[int index]
        {
            get
            {
                if(0 <= index && index < this.Count)
                {
                    return this.Readers[index];
                }
                return null;
            }
        }

        public bool Ready
        {
            get
            {
                return this.Count >= 5 && this.Readers
                    .Select((reader) => reader != null && reader.Ready)
                    .All((ready) => ready);
            }
        }

        internal void RefreshValues()
        {
            lock(this.Readers)
            {
                foreach(var reader in this.Readers)
                {
                    reader.RefreshValue();
                }
            }
        }

        internal void Test(Random r)
        {
            int index = r.Next(this.Readers.Count);
            this.Readers[index]?.Test(r);
        }

        public async Task Connect(string description, GattCharacteristic sensor)
        {
            if(sensor.Uuid == GATTDefaultCharacteristic.Analog.UUID && sensor.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            {
                byte index = (byte)names.IndexOf(description);
                byte firstValue = await Glove.GetValue(sensor);
                var reader = this.Readers[index];
                if (reader.Name != description)
                {
                    throw new Exception(string.Format("WHAT THE HELL JUST HAPPENED? Expected {0}, but got {1}", reader.Name, description));
                }
                await reader.Connect(sensor);
            }
        }

        public void CalibrateMin(int index, byte value)
        {
            this.Readers[index]?.CalibrateMin(value);
        }

        public void CalibrateMin(int index)
        {
            this.Readers[index]?.CalibrateMin();
        }

        public void CalibrateMin()
        {
            for(int i = 1; i < this.Count; ++i)
            {
                this.CalibrateMin(i);
            }
        }

        public void CalibrateMax(int index, byte value)
        {
            this.Readers[index]?.CalibrateMax(value);
        }

        public void CalibrateMax(int index)
        {
            this.Readers[index]?.CalibrateMax();
        }

        public void CalibrateMax()
        {
            for(int i = 1; i < this.Count; ++i)
            {
                this.CalibrateMax(i);
            }
        }

        public bool IsConnectable(string description)
        {
            return names.Contains(description)
                && this.Readers.All((r) => r.Name != description);
        }
    }
}
