using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace HapticGlove
{
    public class Glove
    {
        private const string BLE_DEVICE_FILTER = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";
        private const string DEVICE_NAME = "NotionTheory Haptic Glove";

        public static Glove DEFAULT;

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

            DEFAULT = new Glove();
            DEFAULT.Search();
        }

        public static byte GetByte(IBuffer stream)
        {
            var buffer = new byte[stream.Length];
            DataReader.FromBuffer(stream).ReadBytes(buffer);
            var b = buffer[0];
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

        public static async Task<byte> GetValue(GattCharacteristic c)
        {
            var result = await c.ReadValueAsync();
            return GetByte(result.Value);
        }

        public GloveState State
        {
            get;
            private set;
        }

        public float Battery
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        public FingerState Fingers
        {
            get;
            private set;
        }

        public MotorState Motors
        {
            get;
            private set;
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

        private DeviceWatcher watcher;
        private Dictionary<string, DeviceInformation> devices;
        private GattCharacteristic batteryCharacteristic;
        private Dictionary<string, string> properties;

        public Glove()
        {
            this.Fingers = new FingerState();
            this.Motors = new MotorState();
            this.State = GloveState.NotReady;
            this.devices = new Dictionary<string, DeviceInformation>();
            this.properties = new Dictionary<string, string>();
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

        private async void ConnectIfPaired(DeviceInformation device)
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
                    this.Connect();
                }
            }
        }

        private void Watcher_Added(DeviceWatcher sender, DeviceInformation device)
        {
            if(!this.devices.ContainsKey(device.Id))
            {
                this.devices.Add(device.Id, device);
                this.ConnectIfPaired(device);
            }
        }

        private void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(this.devices.ContainsKey(deviceUpdate.Id))
            {
                var device = this.devices[deviceUpdate.Id];
                device.Update(deviceUpdate);
                this.ConnectIfPaired(device);
            }
        }

        private void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(this.devices.ContainsKey(deviceUpdate.Id))
            {
                this.devices[deviceUpdate.Id].Update(deviceUpdate);
                this.devices.Remove(deviceUpdate.Id);
            }
        }

        private void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
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

        private void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            this.watcher.Stopped -= Watcher_Stopped;
            this.watcher = null;
            this.State &= ~GloveState.Watching;
        }

        private async Task<DeviceInformation> GetService(GATTDefaultService service)
        {
            return (from device in await DeviceInformation.FindAllAsync(service.Filter)
                    where device.Name == DEVICE_NAME
                    select device).FirstOrDefault();
        }

        public async void Connect()
        {
            this.Error = null;
            this.State |= GloveState.Searching;
            try
            {
                var deviceInformationService = await GetService(GATTDefaultService.DeviceInformation);
                if(deviceInformationService != null)
                {
                    ReadDeviceInformation(await GattDeviceService.FromIdAsync(deviceInformationService.Id));
                    this.State |= GloveState.DeviceInformationServiceFound;
                }

                var batteryService = await GetService(GATTDefaultService.BatteryService);
                if(batteryService != null)
                {
                    this.State |= GloveState.BatteryServiceFound;
                    ReadBatteryService(await GattDeviceService.FromIdAsync(batteryService.Id));
                }
            }
            catch(Exception exp)
            {
                this.Error = exp;
            }
            finally
            {
                this.State &= ~GloveState.Searching;
            }
        }

        private async void ReadDeviceInformation(GattDeviceService genericAccess)
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

        private async void ReadBatteryService(GattDeviceService deviceService)
        {
            var characteristics = deviceService.GetAllCharacteristics();
            foreach(var characteristic in characteristics)
            {
                if(characteristic.Uuid == GATTDefaultCharacteristic.BatteryLevel.UUID)
                {
                    this.batteryCharacteristic = characteristic;
                    await this.batteryCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    this.batteryCharacteristic.ValueChanged += BatteryCharacteristic_ValueChanged;
                    this.State |= GloveState.BatteryFound;
                }
                else
                {
                    var description = await GetDescription(characteristic);
                    if(description != null)
                    {
                        if(description.StartsWith("Motor "))
                        {
                            await this.Motors.Connect(description, characteristic);
                            if(this.Motors.Ready)
                            {
                                this.State |= GloveState.MotorsFound;
                            }
                        }
                        else if(description.StartsWith("Sensor "))
                        {
                            await this.Fingers.Connect(description, characteristic);
                            for(int i = 0; i < FingerStates.Length; ++i)
                            {
                                if(this.Fingers.HasFinger(i))
                                {
                                    this.State |= FingerStates[i];
                                }
                            }
                        }
                    }
                }
            }
        }

        private static GloveState[] FingerStates = new GloveState[]
        {
            GloveState.Finger1Found,
            GloveState.Finger2Found,
            GloveState.Finger3Found,
            GloveState.Finger4Found,
            GloveState.Finger5Found
        };

        private void BatteryCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            this.Battery = GetByte(args.CharacteristicValue);
        }
    }
}