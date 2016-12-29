using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HapticGloveServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string BLE_DEVICE_FILTER = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";
        private const string DEVICE_NAME = "NotionTheory Haptic Glove";

        private static GATTDefaultService[] SERVICES;

        static MainPage()
        {
            SERVICES = new GATTDefaultService[] {
                GATTDefaultService.BatteryService,
                GATTDefaultService.DeviceInformation,
                GATTDefaultService.GenericAccess,
                GATTDefaultService.GenericAttribute,
                new GATTDefaultService("Custom", GATTDefaultService.MakeGATTFilter(new Guid("{40792AF0-B0A9-4173-B72D-5488AB301DB5}")))
            };
        }

        DeviceWatcher watcher;
        Dictionary<string, DeviceInformation> devices;

        public MainPage()
        {
            this.InitializeComponent();
            devices = new Dictionary<string, DeviceInformation>();
            Task.Run(StartWatcher);
        }

        private void Write(string msg = "")
        {
            this.Description += msg;
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.Bindings.Update();
            }).AsTask().Wait();
        }

        private void WriteLine(string msg = "")
        {
            Write(msg + "\n");
        }

        private void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        private void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        private async Task StartWatcher()
        {
            WriteLine("Watching for devices.");
            devices.Clear();
            watcher = DeviceInformation.CreateWatcher(BLE_DEVICE_FILTER, null, DeviceInformationKind.AssociationEndpoint);
            watcher.Added += Watcher_Added;
            watcher.Updated += Watcher_Updated;
            watcher.Removed += Watcher_Removed;
            watcher.Stopped += Watcher_Stopped;
            watcher.Start();
        }

        private void StopWatcher()
        {
            watcher.Added -= Watcher_Added;
            watcher.Updated -= Watcher_Updated;
            watcher.Removed -= Watcher_Removed;
            watcher.Stopped -= Watcher_Stopped;
            if(DeviceWatcherStatus.Started == watcher.Status ||
                DeviceWatcherStatus.EnumerationCompleted == watcher.Status)
            {
                watcher.Stop();
            }
            watcher = null;
        }

        public string Description
        {
            get; set;
        }

        private bool running = false;
        private async Task GetServices(DeviceInformation deviceInfo)
        {
            if(!running)
            {
                running = true;
                try
                {
                    if(!deviceInfo.Pairing.IsPaired)
                    {
                        WriteLine(" not paired.");
                    }
                    else
                    {
                        foreach(var service in SERVICES)
                        {
                            Write("\nFinding services [{0}]... ", service.Description);
                            var serviceDevices = (from device in await DeviceInformation.FindAllAsync(service.Filter)
                                                  where device.Name == DEVICE_NAME
                                                  select device).ToArray();
                            if(serviceDevices.Length == 0)
                            {
                                WriteLine("not found.");
                            }
                            else
                            {
                                WriteLine("found.");
                                foreach(var serviceDevice in serviceDevices)
                                {
                                    await EnumerateService(serviceDevice);
                                }
                            }
                        }
                    }
                }
                catch(Exception exp)
                {
                    WriteLine("ERROR: {0}", exp.Message);
                }
                running = false;
            }
        }

        private async Task EnumerateService(DeviceInformation device)
        {
            try
            {
                var deviceService = await GattDeviceService.FromIdAsync(device.Id);
                if(deviceService == null)
                {
                    WriteLine("No device service found.");
                }
                else
                {
                    await EnumerateCharacteristics(deviceService);
                }
            }
            catch(FileNotFoundException exp)
            {
                WriteLine("Failed to acquire device information. {0}", exp.Message);
            }
        }

        private async Task EnumerateCharacteristics(GattDeviceService deviceService)
        {
            var characteristics = deviceService.GetAllCharacteristics();
            WriteLine("{0} characteristics found.", characteristics.Count);
            foreach(var c in characteristics)
            {
                GattReadResult result = null;
                if(c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
                {
                    result = await c.ReadValueAsync();
                }

                PrintResult("\t", c.Uuid, result);

                var descriptors = c.GetAllDescriptors();
                foreach(var descriptor in descriptors)
                {
                    PrintResult("\t\t", descriptor.Uuid, await descriptor.ReadValueAsync());
                }
            }
        }

        private void PrintResult(string pre, Guid uuid, GattReadResult result)
        {
            Write("{0}[{1}]: ", pre, GATTDefaultCharacteristic.Find(uuid)?.Description ?? uuid.ToString());
            var str = "";
            if(result == null)
            {
                str = "Not readable";
            }
            else
            {
                try
                {
                    var buffer = new byte[result.Value.Length];
                    DataReader.FromBuffer(result.Value).ReadBytes(buffer);
                    if(buffer.Length > 0 && buffer[0] >= 0x20)
                    {
                        str = Encoding.ASCII.GetString(buffer);
                    }
                    else
                    {
                        switch(buffer.Length)
                        {
                        case 1:
                            str = buffer[0].ToString();
                            break;
                        case 2:
                            ushort shortVal = (ushort)(buffer[0] << 8 | buffer[1]);
                            str = shortVal.ToString();
                            break;
                        case 4:
                            uint intVal = (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
                            str = intVal.ToString();
                            break;
                        default:
                            str = "<" + string.Join("|", buffer.Select(b => b.ToString("X2"))) + ">";
                            break;
                        }
                    }
                }
                catch(Exception exp)
                {
                    str = "ERROR: " + exp.Message;
                }
            }

            WriteLine("[{0}] {1}", result?.Status.ToString() ?? "Failed", str);
        }

        private async void Watcher_Added(DeviceWatcher watcher, DeviceInformation deviceInfo)
        {
            if(!devices.ContainsKey(deviceInfo.Id))
            {
                devices.Add(deviceInfo.Id, deviceInfo);
                Write("New device => {0}... ", deviceInfo.Name);
                if(deviceInfo.Name == DEVICE_NAME)
                {
                    if(!deviceInfo.Pairing.IsPaired)
                    {
                        Write("Pairing... ");
                        await deviceInfo.Pairing.PairAsync();
                        WriteLine("paired.");
                    }
                    else
                    {
                        WriteLine("already paired.", deviceInfo.Name);
                    }

                    await GetServices(deviceInfo);
                }
            }
        }

        private async void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                var deviceInfo = devices[deviceUpdate.Id];
                Write("Device updated => {0}", deviceInfo.Name);
                await GetServices(deviceInfo);
            }
        }

        private void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                var deviceInfo = devices[deviceUpdate.Id];
                WriteLine("Device removed {0}: Paired => {1}.", deviceInfo.Name, deviceInfo.Pairing.IsPaired);
                devices.Remove(deviceUpdate.Id);
            }
        }

        private void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            WriteLine("No longer searching for devices");
        }
    }
}
