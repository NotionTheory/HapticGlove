using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using ValueType = System.UInt16;

namespace HapticGlove
{
    public class Sensor : INotifyPropertyChanged
    {
        public Sensor(string name, ValueType firstValue, int index, MotorState motorState)
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.Index = index;
            this.motorState = motorState;
            this.Name = name;
            this._min = ValueType.MaxValue;
            this._max = ValueType.MinValue;
            this.valueFound = false;
            this.SetValue(firstValue);
        }

        public float LerpA
        {
            get;
            set;
        }

        float LerpB
        {
            get
            {
                return 1 - LerpA;
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

        private ValueType _min, _max;
        public ValueType Min
        {
            get
            {
                return this._min;
            }
        }

        private void SetMin(ValueType value)
        {
            if(this._min != value)
            {
                this._min = value;
                this.OnPropertyChanged("Min");
                this.RefreshValue();
            }
        }

        public ValueType Max
        {
            get
            {
                return this._max;
            }
        }

        private void SetMax(ValueType value)
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

        private ValueType _reading;
        public ValueType Reading
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
            this.Value = this.Value * LerpA + this.target * LerpB;
        }

        private MotorState motorState;
        public int Index
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

        public void CalibrateMin(ValueType value)
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

        public void CalibrateMax(ValueType value)
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

        private void SetValue(ValueType b)
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
                        this.SetMin((ValueType)(this._min - 1));
                    }
                    else
                    {
                        this.SetMax((ValueType)(this._max + 1));
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
                this.SetValue(Glove.GetNumber(args.CharacteristicValue));
            };
        }

        public void Test(Random r)
        {
            if(!this.Ready)
            {
                this.SetValue((ValueType)r.Next(25, 75));
            }
        }
    }
}
