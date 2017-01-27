using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;

namespace HapticGlove
{
    public class FingerState : IList<float>, INotifyCollectionChanged
    {
        private static Regex indexPattern = new Regex("^Sensor (\\d+)$");
        private List<float> values;
        private List<float> testValues;
        private List<GattCharacteristic> sensors;

        public FingerState()
        {
            this.values = new List<float>();
            this.testValues = new List<float>();
            this.sensors = new List<GattCharacteristic>();
            for(int i = 0; i < 5; ++i)
            {
                this.testValues.Add(0);
            }
        }

        public bool HasFinger(int index)
        {
            return 0 <= index && index < this.sensors.Count && this.sensors[index] != null;
        }

        public int Count
        {
            get
            {
                return Math.Min(this.sensors.Count, this.values.Count);
            }
        }

        public float this[int index]
        {
            get
            {
                var values = this.Ready ? this.values : this.testValues;
                if(0 <= index && index < values.Count)
                {
                    return values[index];
                }
                return 0f;
            }

            set
            {
                var values = this.Ready ? this.values : this.testValues;
                if(0 <= index && index < values.Count)
                {
                    values[index] = value;
                    this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
                }
            }
        }

        public bool Ready
        {
            get
            {
                return this.Count >= 5;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private static int GetIndex(string description)
        {
            var match = indexPattern.Match(description);
            if(match != null && match.Groups.Count > 1)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return -1;
        }

        internal void Test(Random r)
        {
            this[r.Next(this.Count)] = (float)r.NextDouble();
        }

        public event EventHandler FingersChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public async Task Connect(string description, GattCharacteristic sensor)
        {
            if(sensor.Uuid == GATTDefaultCharacteristic.Analog.UUID && sensor.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            {
                var index = GetIndex(description);
                if(0 <= index)
                {
                    while(index >= this.sensors.Count)
                    {
                        this.sensors.Add(null);
                        this.values.Add(0f);
                    }
                    if(this.sensors[index] == null)
                    {
                        this.sensors[index] = sensor;
                        this.values[index] = await Glove.GetValue(sensor) / 256f;

                        await sensor.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        sensor.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
                        {
                            this.values[index] = Glove.GetByte(args.CharacteristicValue) / 256f;
                            this.FingersChanged?.Invoke(this, EventArgs.Empty);
                        };
                    }
                }
            }
        }

        public int IndexOf(float item)
        {
            var values = this.Ready ? this.values : this.testValues;
            return values.IndexOf(item);
        }

        public void Insert(int index, float item)
        {
            var values = this.Ready ? this.values : this.testValues;
            values.Insert(index, item);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            var values = this.Ready ? this.values : this.testValues;
            var oldValue = this[index];
            values.RemoveAt(index);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValue, index));
        }

        public void Add(float item)
        {
            var values = this.Ready ? this.values : this.testValues;
            values.Add(item);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, this.Count - 1));
        }

        public void Clear()
        {
            var values = this.Ready ? this.values : this.testValues;
            values.Clear();
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(float item)
        {
            var values = this.Ready ? this.values : this.testValues;
            return values.Contains(item);
        }

        public void CopyTo(float[] array, int arrayIndex)
        {
            var values = this.Ready ? this.values : this.testValues;
            values.CopyTo(array, arrayIndex);
        }

        public bool Remove(float item)
        {
            var values = this.Ready ? this.values : this.testValues;
            var index = this.IndexOf(item);
            var removed = values.Remove(item);
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return removed;
        }

        public IEnumerator<float> GetEnumerator()
        {
            var values = this.Ready ? this.values : this.testValues;
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var values = this.Ready ? this.values : this.testValues;
            return values.GetEnumerator();
        }
    }
}
