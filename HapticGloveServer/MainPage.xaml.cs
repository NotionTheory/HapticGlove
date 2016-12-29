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
using System.Threading;

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

        static MainPage()
        {
            //new GATTDefaultService("Nordic Serial Emulator", new Guid("{6e400001-b5a3-f393-e0a9-e50e24dcca9e}"));
            //new GATTDefaultService("Custom", new Guid("{40792AF0-B0A9-4173-B72D-5488AB301DB5}"));
            //new GATTDefaultService("Unknown 1", new Guid("{00001530-1212-efde-1523-785feabcd123}"));
            //new GATTDefaultService("Unknown 2", new Guid("{ee0c2080-8786-40ba-ab96-99b91ac981d8}"));
        }

        DeviceWatcher watcher;
        Dictionary<string, DeviceInformation> devices;
        Dictionary<string, GattCharacteristic> watchedCharacteristics;

        public MainPage()
        {
            this.InitializeComponent();
            devices = new Dictionary<string, DeviceInformation>();
            watchedCharacteristics = new Dictionary<string, GattCharacteristic>();
            Task.Run(StartWatcher);
        }

        private void Write(string msg = "")
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.Description += msg;
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

        private async Task GetServices(DeviceInformation deviceInfo)
        {
            try
            {
                if(!deviceInfo.Pairing.IsPaired)
                {
                    WriteLine(" not paired.");
                }
                else
                {
                    WriteLine("For device {0}:", deviceInfo.Name);
                    foreach(var service in GATTDefaultService.All)
                    {
                        var serviceDevices = (from device in await DeviceInformation.FindAllAsync(service.Filter)
                                              where device.Name == DEVICE_NAME
                                              select device).ToArray();
                        if(serviceDevices.Length > 0)
                        {
                            WriteLine("\n\t{0}:", service.Description);
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
                WriteLine("ERROR1: {0}", exp.Message);
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
            var cs = deviceService.GetAllCharacteristics();
            if(cs.Count == 0)
            {
                WriteLine("\t\t<No characteristics found>");
            }
            else
            {
                for(int i = 0; i < cs.Count; ++i)
                {
                    try
                    {
                        var c = cs[i];
                        GattReadResult result = null;
                        if(c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
                        {
                            result = await c.ReadValueAsync();
                        }

                        Write("\t\t");

                        var name = string.Format("{0}_{1}", GATTDefaultCharacteristic.Find(c.Uuid)?.Description ?? c.Uuid.ToString(), i);

                        if(c.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                        {
                            Write("[watch] ");
                            watchedCharacteristics.Add(name, c);
                            c.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
                            {
                                WriteLine("[{1}]: {0}", TranslateStream(args.CharacteristicValue), name);
                            };
                            await c.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        }

                        PrintResult("", c.Uuid, result);
                    }
                    catch(Exception exp)
                    {
                        WriteLine("ERROR3: {0}", exp.Message);
                    }
                }
            }
        }

        private string PrintResult(string pre, Guid? uuid, GattReadResult result)
        {
            Write(pre);
            if(uuid.HasValue)
            {
                Write("{0}: ", GATTDefaultCharacteristic.Find(uuid.Value)?.Description ?? uuid.ToString());
            }

            var str = "";
            if(result == null)
            {
                str = "<Not readable>";
            }
            else
            {
                try
                {
                    str = TranslateStream(result.Value);
                }
                catch(Exception exp)
                {
                    str = "ERROR2: " + exp.Message;
                }
            }

            WriteLine(str);
            return str;
        }

        private static string TranslateStream(IBuffer stream)
        {
            string str;
            var buffer = new byte[stream.Length];
            DataReader.FromBuffer(stream).ReadBytes(buffer);
            switch(buffer.Length)
            {
            case 0:
                str = "<no data>";
                break;
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
                if(buffer[0] >= 0x20)
                {
                    str = Encoding.ASCII.GetString(buffer);
                }
                else
                {
                    str = "<" + string.Join("|", buffer.Select(b => b.ToString("X2"))) + ">";
                }
                break;
            }

            return str;
        }

        private async void Watcher_Added(DeviceWatcher watcher, DeviceInformation deviceInfo)
        {
            if(!devices.ContainsKey(deviceInfo.Id))
            {
                devices.Add(deviceInfo.Id, deviceInfo);
                if(deviceInfo.Name == DEVICE_NAME)
                {
                    if(!deviceInfo.Pairing.IsPaired)
                    {
                        Write("New device {0} pairing... ", deviceInfo.Name);
                        await deviceInfo.Pairing.PairAsync();
                        WriteLine("paired.");
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
            }
        }

        private void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                var deviceInfo = devices[deviceUpdate.Id];
                devices.Remove(deviceUpdate.Id);
            }
        }

        private void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            WriteLine("No longer searching for devices");
        }
    }
}
