using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace HapticGlove
{
    public class MotorState : INotifyPropertyChanged
    {
        private GattCharacteristic motor;
        private byte state, testState;

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
                byte mask = (byte)(1 << index);
                return (state & mask) != 0;
            }
            set
            {
                var state = this.Ready ? this.state : this.testState;
                byte mask = (byte)(1 << index);
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
                    this.Flush().Wait();
                }
                else
                {
                    this.testState = state;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Format("Motor{0}", index)));
                }
            }
        }

        internal void Test(Random r)
        {
            this[r.Next(5)] = r.NextDouble() > 0.5;
        }

        public async Task Flush()
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
