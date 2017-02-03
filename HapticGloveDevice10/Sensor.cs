using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HapticGlove
{
    public class Sensor : INotifyPropertyChanged
    {
        public Sensor(string name, byte firstValue, byte min, byte max, int index, MotorState motorState)
        {
            this.index = index;
            this.motorState = motorState;
            this.Name = name;
            this.arg = new PropertyChangedEventArgs(name);
            this.Min = min;
            this.minSet = this.Min != byte.MaxValue;
            this.Max = max;
            this.maxSet = this.Max != byte.MinValue;
            this.reading = firstValue;
        }

        public readonly string Name;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsFinger
        {
            get
            {
                return this.Name != "Battery";
            }
        }

        public bool Motor
        {
            get
            {
                return this.motorState[index];
            }
            set
            {
                this.motorState[index] = value;
            }
        }

        private bool minSet, maxSet;

        public byte Min
        {
            get;
            private set;
        }

        public byte Max
        {
            get;
            private set;
        }

        private readonly PropertyChangedEventArgs arg;

        private float Delta
        {
            get
            {
                return Max - Min;
            }
        }

        private GattCharacteristic sensor;

        private byte reading;
        private MotorState motorState;
        private int index;

        public float Value
        {
            get
            {
                float value = this.reading;

                if(Delta > 0)
                {
                    value -= Min;
                    value /= Delta;
                    value = Math.Min(1, value);
                    value = Math.Max(0, value);
                }

                return value;
            }
        }

        public void CalibrateMin()
        {
            Min = reading;
            minSet = true;
            this.Changed();
        }

        public void CalibrateMax()
        {
            Max = reading;
            maxSet = true;
            this.Changed();
        }

        private void SetValue(byte b)
        {
            if(!minSet)
            {
                Min = Math.Min(Min, b);
            }

            if(!maxSet)
            {
                Max = Math.Max(Max, b);
            }

            if(Min == Max)
            {
                if(Min > 0)
                {
                    --Min;
                }
                else
                {
                    ++Max;
                }
            }

            if(b != this.reading)
            {
                reading = b;
                this.Changed();
            }
        }

        private void Changed()
        {
            this.PropertyChanged?.Invoke(this, arg);
        }

        public bool Ready
        {
            get
            {
                return this.sensor != null;
            }
        }

        public async Task Connect(GattCharacteristic sensor)
        {
            await sensor.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            this.sensor = sensor;
            this.sensor.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
            {
                this.SetValue(Glove.GetByte(args.CharacteristicValue));
            };
        }

        public void Test(Random r)
        {
            if(!this.Ready)
            {
                this.SetValue((byte)r.Next(this.Min, this.Max));
            }
        }
    }
}
