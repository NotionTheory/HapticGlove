using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Core;

namespace HapticGlove
{
    public class Sensor : INotifyPropertyChanged
    {
        public Sensor(string name, byte firstValue, byte index, MotorState motorState)
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.Index = index;
            this.motorState = motorState;
            this.Name = name;
            this._min = byte.MaxValue;
            this._max = byte.MinValue;
            this.valueFound = false;
            this.SetValue(firstValue);
        }

        private const float LERP_A = 0.5f, LERP_B = 1 - LERP_A;
        Dictionary<string, PropertyChangedEventArgs> propArgs;

        public string Name
        {
            get; private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private async void OnPropertyChanged(string name)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if(!propArgs.ContainsKey(name))
                {
                    propArgs.Add(name, new PropertyChangedEventArgs(name));
                }
                this.PropertyChanged?.Invoke(this, propArgs[name]);
            });
        }

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
                return this.motorState[Index];
            }
            set
            {
                this.motorState[Index] = value;
            }
        }

        private bool minSet, maxSet;

        private byte _min, _max;
        public byte Min
        {
            get
            {
                return this._min;
            }
        }

        private void SetMin(byte value)
        {
            if(this._min != value)
            {
                this._min = value;
                this.OnPropertyChanged("Min");
                this.OnPropertyChanged("Value");
            }
        }

        public byte Max
        {
            get
            {
                return this._max;
            }
        }

        private void SetMax(byte value)
        {
            if(this._max != value)
            {
                this._max = value;
                this.OnPropertyChanged("Max");
                this.OnPropertyChanged("Value");
            }
        }

        private float Delta
        {
            get
            {
                return this.Max - this.Min;
            }
        }

        private GattCharacteristic sensor;

        private byte _reading;
        public byte Reading
        {
            get
            {
                return this._reading;
            }

            private set
            {
                if(value != this._reading)
                {
                    this._reading = value;
                    this.OnPropertyChanged("Reading");
                    this.OnPropertyChanged("Value");
                }
            }
        }

        private MotorState motorState;
        public byte Index
        {
            get; private set;
        }

        private bool valueFound;

        private float _value;
        public float Value
        {
            get
            {
                float value = this.Reading;
                if(this.Delta > 0)
                {
                    value -= this.Min;
                    value /= this.Delta;
                    value = Math.Min(1, value);
                    value = Math.Max(0, value);
                }

                this._value = this._value * LERP_A + value * LERP_B;
                return this._value;
            }
        }

        public void CalibrateMin(byte value)
        {
            if(value != this.Min)
            {
                this.minSet = true;
                this.SetMin(value);
            }
        }

        public void CalibrateMin()
        {
            this.CalibrateMin(this.Reading);
        }

        public void CalibrateMax(byte value)
        {
            if(value != this.Max)
            {
                this.maxSet = true;
                this.SetMax(value);
            }
        }

        public void CalibrateMax()
        {
            this.CalibrateMax(this.Reading);
        }

        private void SetValue(byte b)
        {
            if(b > 0 || this.valueFound)
            {
                this.valueFound = true;
                if(!this.minSet && b < this.Min)
                {
                    this.SetMin(b);
                }

                if(!this.maxSet && b > this.Max)
                {
                    this.SetMax(b);
                }

                if(this.Min == this.Max)
                {
                    if(this.Min > 0)
                    {
                        this.SetMin((byte)(this._min - 1));
                    }
                    else
                    {
                        this.SetMax((byte)(this._max + 1));
                    }
                }

                this.Reading = b;
            }
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
            this.sensor = sensor;
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
                this.SetValue((byte)r.Next(this.Min, this.Max));
            }
        }
    }
}
