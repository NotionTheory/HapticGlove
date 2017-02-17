using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using ValueType = System.Byte;

namespace HapticGlove
{
    public class MotorState : INotifyPropertyChanged
    {
        private GattCharacteristic motor;
        private ValueType state, testState;

        public int Count
        {
            get;
            private set;
        }

        public MotorState()
        {
            this.state = 0;
            this.testState = 0;
        }

        public bool Ready
        {
            get
            {
                return this.Count > 0 && this.motor != null;
            }
        }

        public bool IsConnectable(string description)
        {
            return description == "Motor State" || description == "Motor Count";
        }

        public async Task Connect(string description, GattCharacteristic c)
        {
            switch(description)
            {
            case "Motor State":
                if(this.motor == null)
                {
                    this.motor = c;
                }
                break;
            case "Motor Count":
                if(this.Count == 0)
                {
                    this.Count = await Glove.GetValue(c);
                }
                break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public bool this[int index]
        {
            get
            {
                var state = this.Ready ? this.state : this.testState;
                ValueType mask = (ValueType)(1 << index);
                return (state & mask) != 0;
            }
            set
            {
                var state = this.Ready ? this.state : this.testState;
                ValueType mask = (ValueType)(1 << index);
                if(value)
                {
                    state |= mask;
                }
                else if(this[index])
                {
                    state ^= mask;
                }
                if(this.Ready)
                {
                    this.state = state;
                    this.Flush();
                }
                else
                {
                    this.testState = state;
                    this.OnPropertyChanged(index);
                }
            }
        }

        internal void SetState(ValueType motorState)
        {
            this.state = motorState;
            this.Flush();
            for(int i = 0; i < this.Count; ++i)
            {
                this.OnPropertyChanged(i);
            }
        }

        void OnPropertyChanged(int index)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Format("Motor{0}", index)));
        }

        internal void Test(Random r)
        {
            this[r.Next(5)] = r.NextDouble() > 0.5;
        }

        public async void Flush()
        {
            if(this.motor != null)
            {
                using(var x = new DataWriter())
                {
                    x.WriteByte(this.state);
                    var buffer = x.DetachBuffer();
                    await this.motor.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
                }
            }
        }
    }
}
