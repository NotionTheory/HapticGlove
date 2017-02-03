using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

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

        internal void Test(Random r)
        {
            int index = r.Next(this.Readers.Count);
            this.Readers[index]?.Test(r);
        }

        public async Task Connect(string description, GattCharacteristic sensor)
        {
            if(sensor.Uuid == GATTDefaultCharacteristic.Analog.UUID && sensor.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            {
                int index = names.IndexOf(description);
                byte firstValue = await Glove.GetValue(sensor),
                    min = byte.MaxValue,
                    max = byte.MinValue;
                if(description == "Battery")
                {
                    min = MIN_BATTERY;
                    max = MAX_BATTERY;
                }
                var reader = new Sensor(description, firstValue, min, max, index, motorState);
                await reader.Connect(sensor);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.Readers.Add(reader);
                });
            }
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
            return names.Contains(description);
        }
    }
}
