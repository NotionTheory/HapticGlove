using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

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

        const float LERP_A = 0.85f;
        float LERP_B
        {
            get
            {
                return 1 - LERP_A;
            }
        }

        Dictionary<string, PropertyChangedEventArgs> propArgs;

        public string Name
        {
            get; private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            Glove.Invoke(() =>
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
                this.RefreshValue();
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
                this.RefreshValue();
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
                    this.target = this.Reading;
                    if(this.Delta > 0)
                    {
                        this.target -= this.Min;
                        this.target /= this.Delta;
                        this.target = Math.Min(1, this.target);
                        this.target = Math.Max(0, this.target);
                    }
                    this.RefreshValue();
                }
            }
        }

        public void RefreshValue()
        {            
            this.Value = this.Value * LERP_A + this.target * LERP_B;
        }

        private MotorState motorState;
        public byte Index
        {
            get; private set;
        }

        private bool valueFound;

        private float target, _value;
        public float Value
        {
            private set
            {
                if(value != this._value)
                {
                    this._value = value;
                    this.OnPropertyChanged("Value");
                }
            }
            get
            {
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
                this.SetValue((byte)r.Next(25, 75));
            }
        }
    }
}
