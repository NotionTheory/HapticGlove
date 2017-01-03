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

        public static Glove DEFAULT;

        private static DeviceWatcher watcher;
        private static Dictionary<string, DeviceInformation> devices;

        static Glove()
        {
            DEFAULT = new Glove();

            devices = new Dictionary<string, DeviceInformation>();

            // This isn't strictly necessary, but it's helpful when trying to figure out what the Adafruit Feather M0 is doing.
            new GATTDefaultService("Nordic UART", new Guid("{6e400001-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultCharacteristic("RX Buffer", new Guid("{6e400003-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultCharacteristic("TX Buffer", new Guid("{6e400002-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultService("Nordic Device Firmware Update Service", new Guid("{00001530-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Packet", new Guid("{00001532-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Control Point", new Guid("{00001531-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Version", new Guid("{00001534-1212-efde-1523-785feabcd123}"));

            watcher = DeviceInformation.CreateWatcher();
            watcher.Added += Watcher_Added;
            watcher.Updated += Watcher_Updated;
            watcher.Removed += Watcher_Removed;
            watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
            watcher.Start();
        }

        private static void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            foreach(var device in devices.Values)
            {
                if(device.Name == Glove.DEVICE_NAME)
                {
                    DEFAULT.Connect();
                }
            }
        }

        private static void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                devices[deviceUpdate.Id].Update(deviceUpdate);
                devices.Remove(deviceUpdate.Id);
            }
        }

        private static void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                devices[deviceUpdate.Id].Update(deviceUpdate);
            }
        }

        private static void Watcher_Added(DeviceWatcher sender, DeviceInformation device)
        {
            if(!devices.ContainsKey(device.Id))
            {
                devices.Add(device.Id, device);
            }
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
            return Encoding.Unicode.GetString(buffer, 0, buffer.Length);
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

        public const string DEVICE_NAME = "NotionTheory Haptic Glove";

        public bool Found
        {
            get;
            private set;
        }

        public bool Ready
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

        public Glove()
        {
            this.Fingers = new FingerState();
            this.Motors = new MotorState();
            this.Ready = true;
        }

        private static GATTDefaultService[] SERVICES = new GATTDefaultService[]
        {
            GATTDefaultService.BatteryService
        };

        public async Task Connect()
        {
            this.Ready = false;
            this.Found = false;
            this.Error = null;
            try
            {
                var deviceService = (from dev in await DeviceInformation.FindAllAsync(GATTDefaultService.BatteryService.Filter)
                                      where dev.Id == DEVICE_NAME
                                      select dev).FirstOrDefault();
                if(deviceService != null)
                {
                    await FindFingersAndMotors(await GattDeviceService.FromIdAsync(deviceService.Id));
                }
                this.Found = this.Fingers.Count > 0;
            }
            catch(Exception exp)
            {
                this.Error = exp;
            }
            finally
            {
                this.Ready = !this.Found;
            }
        }

        private async Task FindFingersAndMotors(GattDeviceService deviceService)
        {
            var cs = deviceService.GetCharacteristics(GATTDefaultCharacteristic.Analog.UUID);
            var numChars = cs.Count;
            if(numChars == 0)
            {
                //throw new Exception("Didn't find any characteristics");
            }
            else
            {

            }
            foreach(var c in cs)
            {
                try
                {
                    var description = await GetDescription(c);
                    if(description != null)
                    {
                        if(description.StartsWith("Motor "))
                        {
                            await this.Motors.Connect(description, c);
                        }
                        else if(description.StartsWith("Sensor "))
                        {
                            await this.Fingers.Connect(description, c);
                        }
                    }
                }
                catch(Exception exp)
                {
                    // skip processing the error;
                }
            }
        }
    }
}
