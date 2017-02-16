using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;
using ValueType = System.UInt16;

namespace HapticGlove
{
    public class Glove : INotifyPropertyChanged
    {
        const string BLE_DEVICE_FILTER = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";
        const string DEVICE_NAME = "NotionTheory Haptic Glove";

        static Glove()
        {
            // This isn't strictly necessary, but it's helpful when trying to figure out what the Adafruit Feather M0 is doing.
            new GATTDefaultService("Nordic UART", new Guid("{6e400001-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultCharacteristic("RX Buffer", new Guid("{6e400003-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultCharacteristic("TX Buffer", new Guid("{6e400002-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultService("Nordic Device Firmware Update Service", new Guid("{00001530-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Packet", new Guid("{00001532-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Control Point", new Guid("{00001531-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Version", new Guid("{00001534-1212-efde-1523-785feabcd123}"));
        }

        public static ValueType GetNumber(IBuffer stream)
        {
            var buffer = new byte[stream.Length];
            DataReader.FromBuffer(stream).ReadBytes(buffer);
            ValueType b = 0;
            for(int i = buffer.Length - 1; i >= 0; --i)
            {
                b <<= 8;
                b |= buffer[i];
            }
            return b;
        }

        public static string GetString(IBuffer stream)
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

        public static async Task<string> GetDescription(GattCharacteristic c)
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

        public void CalibrateMax()
        {
            this.Sensors.CalibrateMax();
        }

        public void CalibrateMin()
        {
            this.Sensors.CalibrateMin();
        }

        public void CalibrateMax(int index)
        {
            this.Sensors.CalibrateMax(index);
        }

        public void CalibrateMax(int index, ValueType value)
        {
            this.Sensors.CalibrateMax(index, value);
        }

        public void CalibrateMin(int index)
        {
            this.Sensors.CalibrateMin(index);
        }

        public void CalibrateMin(int index, ValueType value)
        {
            this.Sensors.CalibrateMin(index, value);
        }

        public static async Task<ValueType> GetValue(GattCharacteristic c)
        {
            var result = await c.ReadValueAsync();
            return GetNumber(result.Value);
        }

        public Sensor this[int index]
        {
            get
            {
                return this.Sensors[index];
            }
        }

        GloveState _state;

        public GloveState State
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
                if(this.State.HasFlag(GloveState.Ready))
                {
                    return this.SoftwareRevision;
                }
                else
                {
                    return string.Join(Environment.NewLine, this.State.ToString()
                        .Split(',')
                        .Select((str) => str.Trim()));
                }
            }
        }

        public SensorState Sensors
        {
            get;
            private set;
        }

        readonly MotorState Motors;

        public void SetMotorState(byte motorState)
        {
            this.Motors.SetState(motorState);
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

        public bool TestMode { get; set; }

        DeviceWatcher watcher;
        Dictionary<string, DeviceInformation> devices;
        Dictionary<string, string> properties;
        Random r;
        Dictionary<string, PropertyChangedEventArgs> propArgs;

        static GloveState[] SensorStates = new GloveState[]
        {
            GloveState.Sensor0Found,
            GloveState.Sensor1Found,
            GloveState.Sensor2Found,
            GloveState.Sensor3Found,
            GloveState.Sensor4Found
        };

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            Invoke(() =>
            {
                if(!propArgs.ContainsKey(name))
                {
                    propArgs.Add(name, new PropertyChangedEventArgs(name));
                }
                this.PropertyChanged?.Invoke(this, propArgs[name]);
            });
        }

        internal static async void Invoke(Action action)
        {
            try
            {
                await System.Windows.Application.Current?.Dispatcher?.InvokeAsync(action);
            }
            catch(TaskCanceledException) { }
            catch(NullReferenceException) { }
        }

        public Glove()
        {
            r = new Random();
            this.devices = new Dictionary<string, DeviceInformation>();
            this.properties = new Dictionary<string, string>();
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.State = GloveState.NotReady;
            this.Motors = new MotorState();
            this.Motors.PropertyChanged += child_PropertyChanged;
            this.Sensors = new SensorState(this.Motors);
            this.InterpolationFactor = 0.85;
        }

        double lerpA;
        public double InterpolationFactor
        {
            get
            {
                return this.lerpA;
            }
            set
            {
                this.lerpA = value;
                foreach(var reader in this.Sensors.Readers)
                {
                    reader.LerpA = (float)value;
                }
            }
        }

        public void Update()
        {
            if(this.State.HasFlag(GloveState.Ready))
            {
                this.Sensors.RefreshValues();
            }
            else
            {
                this.Test();
            }
        }

        void child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        public void Test()
        {
            if (this.TestMode)
            {
                this.Motors.Test(r);
                this.Sensors.Test(r);
            }
        }

        public void Search()
        {
            this.devices.Clear();
            this.State = GloveState.Watching;

            if(this.watcher == null)
            {
                this.watcher = DeviceInformation.CreateWatcher(BLE_DEVICE_FILTER, null, DeviceInformationKind.AssociationEndpoint);
                this.watcher.Added += Watcher_Added;
                this.watcher.Updated += Watcher_Updated;
                this.watcher.Removed += Watcher_Removed;
                this.watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
                this.watcher.Stopped += Watcher_Stopped;
            }
            if(this.watcher.Status != DeviceWatcherStatus.Started)
            {
                this.watcher.Start();
            }
        }

        async Task ConnectIfPaired(DeviceInformation device)
        {
            if(device.Name == DEVICE_NAME)
            {
                if(!device.Pairing.IsPaired && device.Pairing.CanPair)
                {
                    await device.Pairing.PairAsync(DevicePairingProtectionLevel.None);
                }
                else if(device.Pairing.IsPaired && !this.State.HasFlag(GloveState.Searching))
                {
                    this.State |= GloveState.DeviceFound;
                    try
                    {
                        this.Connect();
                    }
                    catch(Exception)
                    {
                        await device.Pairing.UnpairAsync();
                        this.State = GloveState.Watching;
                    }
                }
            }
        }

        async void Watcher_Added(DeviceWatcher sender, DeviceInformation device)
        {
            if(!this.devices.ContainsKey(device.Id))
            {
                this.devices.Add(device.Id, device);
                await this.ConnectIfPaired(device);
            }
        }

        async void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(this.devices.ContainsKey(deviceUpdate.Id))
            {
                var device = this.devices[deviceUpdate.Id];
                device.Update(deviceUpdate);
                await this.ConnectIfPaired(device);
            }
        }

        void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(this.devices.ContainsKey(deviceUpdate.Id))
            {
                this.devices[deviceUpdate.Id].Update(deviceUpdate);
                this.devices.Remove(deviceUpdate.Id);
            }
        }

        void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            this.watcher.Added -= Watcher_Added;
            this.watcher.Updated -= Watcher_Updated;
            this.watcher.Removed -= Watcher_Removed;
            this.watcher.EnumerationCompleted -= Watcher_EnumerationCompleted;
            if(this.watcher.Status == DeviceWatcherStatus.Started || this.watcher.Status == DeviceWatcherStatus.EnumerationCompleted)
            {
                this.watcher.Stop();
            }
        }

        void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            this.watcher.Stopped -= Watcher_Stopped;
            this.watcher = null;
            this.State &= ~GloveState.Watching;
        }

        async Task<DeviceInformation> GetService(GATTDefaultService service)
        {
            return (from device in await DeviceInformation.FindAllAsync(service.Filter)
                    where device.Name == DEVICE_NAME
                    select device).FirstOrDefault();
        }

        async void Connect()
        {
            if(!this.State.HasFlag(GloveState.Ready))
            {
                this.State |= GloveState.Searching;

                var deviceInformationService = await GetService(GATTDefaultService.DeviceInformation);
                if(deviceInformationService != null)
                {
                    this.State |= GloveState.DeviceInformationServiceFound;
                    var service = await GattDeviceService.FromIdAsync(deviceInformationService.Id);
                    ReadDeviceInformation(service);
                }

                var batteryService = await GetService(GATTDefaultService.BatteryService);
                if(batteryService != null)
                {
                    this.State |= GloveState.BatteryServiceFound;
                    var service = await GattDeviceService.FromIdAsync(batteryService.Id);
                    ReadBatteryService(service);
                }

                this.State &= ~GloveState.Searching;
            }
        }

        async void ReadDeviceInformation(GattDeviceService genericAccess)
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

        async void ReadBatteryService(GattDeviceService deviceService)
        {
            var characteristics = deviceService.GetAllCharacteristics();
            foreach(var characteristic in characteristics)
            {
                var description = await GetDescription(characteristic) ?? "";
                if(this.Motors.IsConnectable(description))
                {
                    await this.Motors.Connect(description, characteristic);
                    if(this.Motors.Ready)
                    {
                        this.State |= GloveState.MotorsFound;
                    }
                }
                else if(this.Sensors.IsConnectable(description))
                {
                    await this.Sensors.Connect(description, characteristic);
                    for(int i = 0; i < SensorStates.Length; ++i)
                    {
                        if(this.Sensors.HasSensor(i))
                        {
                            this.State |= SensorStates[i];
                        }
                    }
                }
            }
        }
    }
}