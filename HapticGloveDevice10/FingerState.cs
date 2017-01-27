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
using System.ComponentModel;

namespace HapticGlove
{
    public class FingerState : INotifyPropertyChanged
    {
        private static Regex indexPattern = new Regex("^Sensor (\\d+)$");
        private List<FloatValueReader> readers;

        public FingerState()
        {
            this.readers = new List<FloatValueReader>();
        }

        public bool HasFinger(int index)
        {
            return 0 <= index && index < this.readers.Count && this.readers[index] != null;
        }

        public int Count
        {
            get
            {
                return this.readers.Count;
            }
        }

        public float this[int index]
        {
            get
            {
                if(0 <= index && index < this.Count)
                {
                    return this.readers[index].Value;
                }
                return 0f;
            }
        }

        public bool Ready
        {
            get
            {
                return this.Count >= 5 && this.readers.All((reader) => reader.Ready);
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
            int index = r.Next(this.readers.Count);
            this.readers[index].Test(r);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Connect(string description, GattCharacteristic sensor)
        {
            if(sensor.Uuid == GATTDefaultCharacteristic.Analog.UUID && sensor.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            {
                var index = GetIndex(description);
                if(0 <= index)
                {
                    while(index >= this.readers.Count)
                    {
                        var reader = new FloatValueReader(string.Format("Finger{0}", index));
                        this.readers.Add(reader);
                        reader.PropertyChanged += Reader_PropertyChanged;
                    }
                    if(!this.readers[index].Ready)
                    {
                        this.readers[index].Connect(sensor);
                    }
                }
            }
        }

        private void Reader_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}
