using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace HapticGlove
{
    public class MotorState
    {
        private GattCharacteristic motor;
        private byte state;

        public int Count
        {
            get;
            private set;
        }

        public MotorState()
        {
            this.state = 0;
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

        public bool this[int index]
        {
            get
            {
                if(0 <= index && index < this.Count)
                {
                    byte mask = (byte)(1 << index);
                    return (this.state & mask) != 0;
                }
                return false;
            }
            set
            {
                if(0 <= index && index < this.Count)
                {
                    byte mask = (byte)(1 << index);
                    if(value)
                    {
                        this.state |= mask;
                    }
                    else if(this[index])
                    {
                        this.state ^= mask;
                    }
                }
            }
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
