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
        //private static readonly ushort SERVICE_ID = 0x1800;
        private static readonly Guid SERVICE_ID = new Guid("{40792AF0-B0A9-4173-B72D-5488AB301DB5}");

        private static string MakeGATTFilter(Guid guid)
        {
            return GattDeviceService.GetDeviceSelectorFromUuid(guid);
        }

        private static string MakeGATTFilter(ushort value)
        {
            return GattDeviceService.GetDeviceSelectorFromShortId(value);
        }

        DeviceWatcher watcher;
        Dictionary<string, DeviceInformation> devices;

        public MainPage()
        {
            this.InitializeComponent();
            devices = new Dictionary<string, DeviceInformation>();
            Task.Run(StartWatcher);
        }

        private async Task StartWatcher()
        {
            await Print("Looking for devices.");
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

        private async Task Print(string format, params object[] args)
        {
            await Print(string.Format(format, args));
        }

        private async Task Print(string msg = "")
        {
            this.Description += msg + "\n";
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.Bindings.Update();
            });
        }

        private async Task GetServices(DeviceInformation deviceInfo, string op)
        {
            var serviceFilter = MakeGATTFilter(SERVICE_ID);
            await Print("\nWhile {1} a device, finding services [filter = {0}]", serviceFilter, op);
            var serviceDevices = (from device in await DeviceInformation.FindAllAsync(serviceFilter)
                                  where device.Name == DEVICE_NAME
                                  select device).ToArray();
            await Print("{0} services found.", serviceDevices.Length);
            foreach(var serviceDevice in serviceDevices)
            {
                await EnumerateService(serviceDevice);
            }
        }

        private async Task EnumerateService(DeviceInformation device)
        {
            await Print("Service Device {0} ({1})", device.Name, device.Id);
            try
            {
                var deviceService = await GattDeviceService.FromIdAsync(device.Id);
                if(deviceService == null)
                {
                    await Print("No device service found.");
                }
                else
                {
                    await EnumerateCharacteristics(deviceService);
                }
            }
            catch(FileNotFoundException exp)
            {
                await Print("Failed to acquire device information. {0}", exp.Message);
            }
        }

        private async Task EnumerateCharacteristics(GattDeviceService deviceService)
        {
            await Print("Service UUID {0}", deviceService.Uuid);
            var characteristics = deviceService.GetAllCharacteristics();
            await Print("{0} characteristics found.", characteristics.Count);
            foreach(var c in characteristics)
            {
                await Print("Characteristic [{0,4:X}]: {1}.", c.AttributeHandle, c.UserDescription);
                //var descrip = await c.ReadClientCharacteristicConfigurationDescriptorAsync();
                //await Print("\tDescription [{0}]: {1}", descrip.Status, descrip.ClientCharacteristicConfigurationDescriptor);
                var value = await c.ReadValueAsync();
                await Print("\tValue [{0}, {2} of {1}]: <{3}>", value.Status, value.Value.Capacity, value.Value.Length, string.Join("|", value.Value.ToArray().Select(b => b.ToString("X"))));
            }
        }

        private async void Watcher_Added(DeviceWatcher watcher, DeviceInformation deviceInfo)
        {
            if(!devices.ContainsKey(deviceInfo.Id))
            {
                devices.Add(deviceInfo.Id, deviceInfo);
                await Print("Device added {0} ({1}): Paired => {2}.", deviceInfo.Name, deviceInfo.Id, deviceInfo.Pairing.IsPaired);
                if(deviceInfo.Name == DEVICE_NAME)
                {
                    if(!deviceInfo.Pairing.IsPaired)
                    {
                        await Print("Attempting to pair {0}...", deviceInfo.Name);
                        await deviceInfo.Pairing.PairAsync();
                        await Print("Device {0} successfully paired.", deviceInfo.Name);
                    }
                    else
                    {
                        await Print("Device {0} already paired.", deviceInfo.Name);
                    }

                    await GetServices(deviceInfo, "adding");
                }
            }
        }

        private async void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                var deviceInfo = devices[deviceUpdate.Id];
                var descrip = string.Join("\n", from kv in deviceUpdate.Properties
                                                select string.Format("\t{0}: {1}", kv.Key, kv.Value));
                await Print("Device updated {0}\n{1}.", deviceInfo.Name, descrip);
                //await GetServices(deviceInfo, "updating");
            }
        }

        private async void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
        {
            if(devices.ContainsKey(deviceUpdate.Id))
            {
                var deviceInfo = devices[deviceUpdate.Id];
                await Print("Device removed {0}: Paired => {1}.", deviceInfo.Name, deviceInfo.Pairing.IsPaired);
                devices.Remove(deviceUpdate.Id);
            }
        }

        private async void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            await Print("No longer searching for devices");
        }
    }
}
