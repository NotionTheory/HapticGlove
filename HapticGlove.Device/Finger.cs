using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace NotionTheory.HapticGlove
{
    public class Finger : INotifyPropertyChanged
    {
        static string[] names = {
            "Thumb",
            "Index",
            "Middle",
            "Ring",
            "Pinkie"
        };
        protected static byte GetNumber(IBuffer stream)
        {
            var buffer = new byte[stream.Length];
            DataReader.FromBuffer(stream).ReadBytes(buffer);
            byte b = 0;
            for(int i = buffer.Length - 1; i >= 0; --i)
            {
                b <<= 8;
                b |= buffer[i];
            }
            return b;
        }

        protected static string GetString(IBuffer stream)
        {
            var buffer = new byte[stream.Length];
            DataReader.FromBuffer(stream).ReadBytes(buffer);
            var test = Encoding.UTF8.GetString(buffer);
            if(buffer.Length > 2 && buffer[1] == 0)
            {
                return Encoding.Unicode.GetString(buffer);
            }
            else
            {
                return Encoding.ASCII.GetString(buffer);
            }
        }

        protected static async Task<string> GetDescription(GattCharacteristic c)
        {
            var descriptor = c.GetDescriptors(GATTDefaultCharacteristic.CharacteristicUserDescription.UUID).FirstOrDefault();
            if(descriptor == null)
            {
                return null;
            }
            else
            {
                var stream = await descriptor.ReadValueAsync();
                return GetString(stream.Value);
            }
        }

        protected static async Task<byte> GetValue(GattCharacteristic c)
        {
            var result = await c.ReadValueAsync();
            return GetNumber(result.Value);
        }
        protected static async void Invoke(Action action)
        {
            try
            {
                await System.Windows.Application.Current?.Dispatcher?.InvokeAsync(action);
            }
            catch(TaskCanceledException) { }
            catch(NullReferenceException) { }
        }


        Dictionary<string, PropertyChangedEventArgs> propArgs;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if(!this.propArgs.ContainsKey(name))
            {
                this.propArgs.Add(name, new PropertyChangedEventArgs(name));
            }
            this.OnPropertyChanged(this, this.propArgs[name]);
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Invoke(() =>
            {
                this.PropertyChanged?.Invoke(sender, e);
            });
        }

        bool minSet, maxSet;
        byte _min, _max;
        GattCharacteristic sensor, motor;
        byte _sensor, _motor;
        bool valueFound;
        int _index;

        public Finger(Hand hand, int index)
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.Hand = hand;
            this.Index = index;
            this._min = byte.MaxValue;
            this._max = byte.MinValue;
            this.valueFound = false;
            this.SetSensor(0);
        }

        public Hand Hand
        {
            get; private set;
        }

        public int Index
        {
            get
            {
                return this._index;
            }
            internal set
            {
                this._index = value;
                this.OnPropertyChanged(nameof(Index));
                this.OnPropertyChanged(nameof(Name));
            }
        }

        public string Name
        {
            get
            {
                return names[this.Index];
            }
        }

        public void OpenFingerTest()
        {
            this.Min = 50;
            this.Max = 100;
            this.Sensor = 51;
        }

        public void CloseFingerTest()
        {
            this.Min = 50;
            this.Max = 100;
            this.Sensor = 99;
        }

        public byte Min
        {
            get
            {
                return this._min;
            }
            set
            {
                this.minSet = true;
                if(this._min != value)
                {
                    this._min = value;
                    this.OnPropertyChanged(nameof(Min));
                    this.OnPropertyChanged(nameof(SensorValue));
                }
            }
        }

        public byte Max
        {
            get
            {
                return this._max;
            }
            set
            {
                this.maxSet = true;
                if(this._max != value)
                {
                    this._max = value;
                    this.OnPropertyChanged(nameof(Max));
                    this.OnPropertyChanged(nameof(SensorValue));
                }
            }
        }

        private float Delta
        {
            get
            {
                return this.Max - this.Min;
            }
        }

        public byte Sensor
        {
            get
            {
                return this._sensor;
            }

            set
            {
                if(value != this._sensor)
                {
                    this._sensor = value;
                    this.OnPropertyChanged(nameof(Sensor));
                    this.OnPropertyChanged(nameof(SensorValue));
                }
            }
        }

        public byte Motor
        {
            get
            {
                return this._motor;
            }

            set
            {
                if(value != this._motor)
                {
                    this._motor = value;
                    WriteMotorState();
                    this.OnPropertyChanged(nameof(Motor));
                    this.OnPropertyChanged(nameof(MotorValue));
                }
            }
        }


        public async Task Connect(string type, GattCharacteristic characteristic)
        {
            if(type == "S")
            {
                this.sensor = characteristic;
                await this.sensor.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                this.sensor.ValueChanged += ReadSensorState;
            }
            else if(type == "M")
            {
                this.motor = characteristic;
            }
        }

        private void ReadSensorState(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            this.SetSensor(GetNumber(args.CharacteristicValue));
        }

        private async void WriteMotorState()
        {
            if(this.motor != null)
            {
                using(var x = new DataWriter())
                {
                    x.WriteByte(this._motor);
                    var buffer = x.DetachBuffer();
                    await this.motor.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
                }
            }
        }

        public float SensorValue
        {
            set
            {
                this.Sensor = (byte)(value * this.Delta + this.Min);
            }
            get
            {
                return Math.Max(0.0f, Math.Min(1.0f, ((float)this._sensor - (float)this.Min) / this.Delta));
            }
        }

        public float MotorValue
        {
            set
            {
                this.Motor = (byte)(Math.Max(0, Math.Min(1, value)) * 255);
            }
            get
            {
                return this.Motor / 255f;
            }
        }

        public void CalibrateMin()
        {
            this.Min = this.Sensor;
        }

        public void CalibrateMax()
        {
            this.Max = this.Sensor;
        }

        private void SetSensor(byte b)
        {
            if(b > 0 || this.valueFound)
            {
                this.valueFound = true;
                var lastMin = this._min;
                var lastMax = this._max;
                if(!this.minSet && b < this.Min)
                {
                    this._min = b;
                }

                if(!this.maxSet && b > this.Max)
                {
                    this._max = b;
                }

                if(this.Min == this.Max)
                {
                    if(this.Min > 0)
                    {
                        this._min = (byte)(this._min - 1);
                    }
                    else
                    {
                        this._max = (byte)(this._max + 1);
                    }
                }

                if(this._min != lastMin)
                {
                    this.OnPropertyChanged(nameof(Min));
                }

                if(this._max != lastMax)
                {
                    this.OnPropertyChanged(nameof(Max));
                }

                this.Sensor = b;
            }
        }

        public bool Ready
        {
            get
            {
                return this.HasSensor && this.HasMotor;
            }
        }

        public bool HasMotor
        {
            get
            {
                return this.motor != null;
            }
        }
        public bool HasSensor
        {
            get
            {
                return this.sensor != null;
            }
        }

        public void TestSensor(Random r)
        {
            if(!this.Ready)
            {
                this.SetSensor((byte)r.Next(25, 75));
            }
        }

        public void TestMotor(Random r, bool clientIsConnected)
        {
            if(!clientIsConnected)
            {
                this.Motor = (byte)r.Next(0, 255);
            }
        }
    }
}
