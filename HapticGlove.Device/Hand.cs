using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace NotionTheory.HapticGlove
{
    public class Hand : INotifyPropertyChanged
    {
        public const int NUM_FINGERS = 5;

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

        public const string DEVICE_NAME = "NotionTheory Haptic Glove";
        static DevicePropertiesSearchState[] SensorStates = new DevicePropertiesSearchState[] {
            DevicePropertiesSearchState.Sensor0Found,
            DevicePropertiesSearchState.Sensor1Found,
            DevicePropertiesSearchState.Sensor2Found,
            DevicePropertiesSearchState.Sensor3Found,
            DevicePropertiesSearchState.Sensor4Found
        };

        static DevicePropertiesSearchState[] MotorStates = new DevicePropertiesSearchState[] {
            DevicePropertiesSearchState.Motor0Found,
            DevicePropertiesSearchState.Motor1Found,
            DevicePropertiesSearchState.Motor2Found,
            DevicePropertiesSearchState.Motor3Found,
            DevicePropertiesSearchState.Motor4Found
        };

        readonly Dictionary<string, string> properties;

        DevicePropertiesSearchState _state;
        Side _side;
        string _defaultID;
        bool _testFingers, _testMotors;

        public Hand()
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this._defaultID = Guid.NewGuid().ToString();
            this.properties = new Dictionary<string, string>();
            this.State = DevicePropertiesSearchState.NotReady;
            this.Side = Side.Right;
            this.Fingers = new ObservableCollection<Finger>();
            for(int i = 0; i < NUM_FINGERS; ++i)
            {
                var finger = new Finger(this, i);
                finger.PropertyChanged += Finger_PropertyChanged;
                this.Fingers.Add(finger);
            }
        }

        internal void Reverse()
        {
            this.Side = 1 - this.Side;
            foreach(var finger in this.Fingers)
            {
                finger.Index = NUM_FINGERS - finger.Index - 1;
            }
        }

        public string ID
        {
            get
            {
                return Device?.Id ?? this._defaultID;
            }
        }

        public DeviceInformation Device
        {
            get; set;
        }

        public Side Side
        {
            get
            {
                return this._side;
            }
            private set
            {
                if(this._side != value)
                {
                    this._side = value;
                    this.OnPropertyChanged(nameof(Side));
                }
            }
        }

        public ObservableCollection<Finger> Fingers
        {
            get; private set;
        }

        public int Count
        {
            get
            {
                return this.Fingers.Count;
            }
        }

        public Finger this[int index]
        {
            get
            {
                if(0 <= index && index < this.Count)
                {
                    return this.Fingers[index];
                }
                return null;
            }
        }

        public void ClearMotorState()
        {
            foreach(var finger in this.Fingers)
            {
                finger.Motor = 0;
            }
        }

        public bool Ready
        {
            get
            {
                return this.Fingers.All(f => f.Ready);
            }
        }

        public DevicePropertiesSearchState State
        {
            get
            {
                return this._state;
            }
            private set
            {
                this._state = value;
                this.OnPropertyChanged(nameof(State));
                this.OnPropertyChanged(nameof(Status));
            }
        }

        public string Status
        {
            get
            {
                if(this.TestFingers || this.TestMotors)
                {
                    return "Test mode";
                }
                else if(this.State.HasFlag(DevicePropertiesSearchState.Ready))
                {
                    return this.SoftwareRevision;
                }
                else
                {
                    return this.State
                        .ToString()
                        .Split(',')
                        .Last();
                }
            }
        }

        public string Manufacturer
        {
            get
            {
                string value = null;
                this.properties.TryGetValue("Manufacturer Name String", out value);
                return value;
            }
        }

        public string ModelNumber
        {
            get
            {
                string value = null;
                this.properties.TryGetValue("Model Number String", out value);
                return value;
            }
        }

        public string SoftwareRevision
        {
            get
            {
                string value = null;
                this.properties.TryGetValue("Software Revision String", out value);
                return value;
            }
        }

        public string FirmwareRevision
        {
            get
            {
                string value = null;
                this.properties.TryGetValue("Firmware Revision String", out value);
                return value;
            }
        }

        public string HardwareRevision
        {
            get
            {
                string value = null;
                this.properties.TryGetValue("Hardware Revision String", out value);
                return value;
            }
        }

        public bool TestFingers
        {
            get
            {
                return this._testFingers;
            }
            set
            {
                this._testFingers = value;
                this.OnPropertyChanged(nameof(Status));
            }
        }

        public bool TestMotors
        {
            get
            {
                return this._testMotors;
            }
            set
            {
                var wasTestMode = this._testMotors;
                this._testMotors = value;
                if(wasTestMode && !this._testMotors)
                {
                    this.ClearMotorState();
                }
                this.OnPropertyChanged(nameof(Status));
            }
        }

        public bool HasSensor(int index)
        {
            return 0 <= index && index < this.Fingers.Count && this.Fingers[index].HasSensor;
        }

        public bool HasMotor(int index)
        {
            return 0 <= index && index < this.Fingers.Count && this.Fingers[index].HasMotor;
        }

        public void CloseFistTest()
        {
            foreach(var finger in this.Fingers)
            {
                finger.CloseFingerTest();
            }
        }

        public void OpenFistTest()
        {
            foreach(var finger in this.Fingers)
            {
                finger.OpenFingerTest();
            }
        }

        public void SetMotor(int index, float value)
        {
            this.Fingers[index].MotorValue = value;
        }

        private void Finger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(sender, e);
        }

        public void Update(Random r, bool clientIsConnected)
        {
            if(this.TestFingers)
            {
                int index = r.Next(this.Fingers.Count);
                this.Fingers[index].TestSensor(r);
            }
            if(this.TestMotors)
            {
                int index = r.Next(this.Fingers.Count);
                this.Fingers[index].TestMotor(r, clientIsConnected);
            }
        }

        async Task<GattDeviceService> GetService(GATTDefaultService service)
        {
            var devices = await DeviceInformation.FindAllAsync(service.Filter);
            foreach(var device in devices)
            {
                if(device.Name == DEVICE_NAME)
                {
                    var deviceService = await GattDeviceService.FromIdAsync(device.Id);
                    if(deviceService.Device.DeviceInformation.Id == this.Device.Id)
                    {
                        return deviceService;
                    }
                }
            }
            return null;
        }

        internal async Task Connect(DeviceInformation device)
        {
            this.Device = device;
            await this.Search();
        }

        internal async Task Search()
        {
            if(!this.State.HasFlag(DevicePropertiesSearchState.Ready))
            {
                this.State |= DevicePropertiesSearchState.DeviceFound;

                var deviceInformationService = await GetService(GATTDefaultService.DeviceInformation);
                if(deviceInformationService != null && !this.State.HasFlag(DevicePropertiesSearchState.DeviceInformationServiceFound))
                {
                    this.State |= DevicePropertiesSearchState.DeviceInformationServiceFound;
                    await ReadDeviceInformation(deviceInformationService);
                }

                var batteryService = await GetService(GATTDefaultService.BatteryService);
                if(batteryService != null && !this.State.HasFlag(DevicePropertiesSearchState.BatteryServiceFound))
                {
                    this.State |= DevicePropertiesSearchState.BatteryServiceFound;
                    await ReadBatteryService(batteryService);
                }
            }
        }

        async Task ReadDeviceInformation(GattDeviceService genericAccess)
        {
            var characteristics = genericAccess.GetAllCharacteristics();
            foreach(var characteristic in characteristics)
            {
                if(characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
                {
                    var description = await GetDescription(characteristic) ?? GATTDefaultCharacteristic.Find(characteristic.Uuid)?.Description ?? characteristic.Uuid.ToString();
                    var result = await characteristic.ReadValueAsync();
                    var value = GetString(result.Value);
                    if(!this.properties.ContainsKey(description))
                    {
                        this.properties.Add(description, value);
                    }
                }
            }
        }

        async Task ReadBatteryService(GattDeviceService deviceService)
        {
            var characteristics = deviceService.GetAllCharacteristics();
            foreach(var characteristic in characteristics)
            {
                var description = await GetDescription(characteristic) ?? "";
                await this.Connect(description, characteristic);
                for(int i = 0; i < this.Fingers.Count; ++i)
                {
                    if(this.HasSensor(i))
                    {
                        this.State |= SensorStates[i];
                    }
                    if(this.HasMotor(i))
                    {
                        this.State |= MotorStates[i];
                    }
                }
            }
        }

        static Regex indexTest = new Regex("(M|S)(\\d)", RegexOptions.Compiled);

        public async Task Connect(string description, GattCharacteristic characteristic)
        {
            if(characteristic.Uuid == GATTDefaultCharacteristic.Analog.UUID && (
                characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read)
                || characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse)))
            {
                var match = indexTest.Match(description);
                if(match != null)
                {
                    string type = match.Groups[1].Value;
                    int index = int.Parse(match.Groups[2].Value);
                    byte firstValue = await GetValue(characteristic);
                    var finger = this.Fingers[index];
                    await finger.Connect(type, characteristic);
                }
            }
        }

        public void CalibrateMin(int index)
        {
            this.Fingers[index].CalibrateMin();
        }

        public void CalibrateMin()
        {
            for(int i = 0; i < this.Count; ++i)
            {
                this.CalibrateMin(i);
            }
        }

        public void CalibrateMax(int index)
        {
            this.Fingers[index].CalibrateMax();
        }

        public void CalibrateMax()
        {
            for(int i = 0; i < this.Count; ++i)
            {
                this.CalibrateMax(i);
            }
        }
    }
}