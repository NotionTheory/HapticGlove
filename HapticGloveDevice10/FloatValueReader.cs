using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HapticGlove
{
    public class FloatValueReader : INotifyPropertyChanged
    {
        private byte min;
        private byte max;
        private readonly PropertyChangedEventArgs arg;

        private float Delta
        {
            get
            {
                return max - min;
            }
        }

        private GattCharacteristic sensor;

        private float _value;
        public float Value
        {
            get
            {
                return this._value;
            }
        }

        private void SetValue(byte b)
        {
            float value = b;
            min = Math.Min(min, b);
            max = Math.Max(max, b);
            value -= min;
            value /= Delta;
            value = Math.Min(1, value);
            value = Math.Max(0, value);
            if(this._value != value)
            {
                this._value = value;
                this.PropertyChanged?.Invoke(this, arg);
            }
        }

        public FloatValueReader(string name, byte min, byte max)
        {
            this.Name = name;
            this.arg = new PropertyChangedEventArgs(name);
            this.min = min;
            this.max = max;
        }

        public FloatValueReader(string name)
            : this(name, byte.MaxValue, byte.MinValue)
        {
        }

        public bool Ready
        {
            get
            {
                return this.sensor != null;
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public async void Connect(GattCharacteristic sensor)
        {
            this.sensor = sensor;
            this.SetValue(await Glove.GetValue(sensor));
            await this.sensor.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            this.sensor.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
            {
                this.SetValue(Glove.GetByte(args.CharacteristicValue));
            };
        }

        public void Test(Random r)
        {
            if(!this.Ready)
            {
                this.SetValue((byte)r.Next(this.min, this.max));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
