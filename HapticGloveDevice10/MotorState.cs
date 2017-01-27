using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace HapticGlove
{
    public class MotorState : IList<bool>, INotifyCollectionChanged
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

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
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

        public event EventHandler MotorsChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
                    this.MotorsChanged?.Invoke(this, EventArgs.Empty);
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, this));
                }
            }
        }

        internal void Test(Random r)
        {
            this[r.Next(this.Count)] = r.NextDouble() > 0.5;
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

        public void Add(bool item)
        {
            int index = this.Count;
            ++this.Count;
            this[index] = item;
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void Clear()
        {
            this.state = 0;
            this.Count = 0;
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(bool item)
        {
            return true;
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            array[arrayIndex] = this[arrayIndex];
        }

        public bool Remove(bool item)
        {
            var index = this.IndexOf(item);
            if(index > -1)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            for(int i = 0; i < this.Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int IndexOf(bool item)
        {
            for(int i = 0; i < this.Count; ++i)
            {
                if(this[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, bool item)
        {
            ++this.Count;
            for(int i = this.Count - 1; i > index; ++i)
            {
                this[i] = this[i - 1];
            }
            this[index] = item;

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            for(int i = index; i < this.Count - 1; ++i)
            {
                this[i] = this[i + 1];
            }
            --this.Count;
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
    }
}
