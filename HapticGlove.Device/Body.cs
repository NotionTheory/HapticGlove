using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace NotionTheory.HapticGlove
{
    public class Body : INotifyPropertyChanged
    {
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

        const int NUM_HANDS = 2;

        const string BLE_DEVICE_FILTER = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";

        static Body()
        {
            //await System.Windows.Application.Current?.Dispatcher?.InvokeAsync(action);
            // This isn't strictly necessary, but it's helpful when trying to figure out what the Adafruit Feather M0 is doing.
            new GATTDefaultService("Nordic UART", new Guid("{6e400001-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultCharacteristic("RX Buffer", new Guid("{6e400003-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultCharacteristic("TX Buffer", new Guid("{6e400002-b5a3-f393-e0a9-e50e24dcca9e}"));
            new GATTDefaultService("Nordic Device Firmware Update Service", new Guid("{00001530-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Packet", new Guid("{00001532-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Control Point", new Guid("{00001531-1212-efde-1523-785feabcd123}"));
            new GATTDefaultCharacteristic("DFU Version", new Guid("{00001534-1212-efde-1523-785feabcd123}"));
        }

        DeviceWatcher watcher;
        Dictionary<string, DeviceInformation> devices;
        Random r;
        DeviceSearchState _state;

        public ObservableCollection<Hand> Hands
        {
            get; set;
        }

        public Body()
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.r = new Random();
            this.State = DeviceSearchState.NotReady;
            this.devices = new Dictionary<string, DeviceInformation>();
            this.Hands = new ObservableCollection<Hand> {
                null,
                null
            };
            lock(this.Hands)
            {
                for(int side = 0; side < NUM_HANDS; ++side)
                {
                    var hand = new Hand();
                    if(side == 1)
                    {
                        hand.Reverse();
                    }
                    hand.PropertyChanged += Hand_PropertyChanged;

                    this.Hands[(int)hand.Side] = hand;
                }
            }
        }


        public void SwapHands()
        {
            lock(this.Hands)
            {
                var swap = this.Hands.Reverse().ToArray();
                for(int i = 0; i < swap.Length; ++i)
                {
                    this.Hands[i] = swap[i];
                    this.Hands[i].Reverse();
                }
            }
        }

        public bool TestFingers
        {
            get
            {
                return this.Hands[0].TestFingers;
            }
            set
            {
                lock(this.Hands)
                {
                    foreach(var hand in this.Hands)
                    {
                        hand.TestFingers = value;
                    }
                }
            }
        }

        public bool TestMotors
        {
            get
            {
                return this.Hands[0].TestMotors;
            }
            set
            {
                lock(this.Hands)
                {
                    foreach(var hand in this.Hands)
                    {
                        hand.TestMotors = value;
                    }
                }
            }
        }

        public void SetMotors(float[] motors)
        {
            lock(this.Hands)
            {
                for(int i = 0; i < motors.Length; ++i)
                {
                    int hand = i / Hand.NUM_FINGERS;
                    int index = i % Hand.NUM_FINGERS;
                    this.Hands[hand].SetMotor(index, motors[i]);
                }
            }
        }

        public void ClearMotorState()
        {
            this.SetMotors(new float[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        }

        private void Hand_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            int handsReady = this.Hands.Count(h => h.State == DevicePropertiesSearchState.Ready);
            if(handsReady == this.Hands.Count)
            {
                this.State |= DeviceSearchState.DevicePropertiesFound;
                this.State &= ~DeviceSearchState.Searching;
            }
            this.OnPropertyChanged(sender, e);
        }

        public void Update(bool clientIsConnected)
        {
            lock(this.Hands)
            {
                foreach(var hand in this.Hands)
                {
                    hand.Update(this.r, clientIsConnected);
                }
            }
        }

        public void Search()
        {
            this.devices.Clear();
            this.State = DeviceSearchState.Watching;

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

        public DeviceSearchState State
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
                return this.State
                    .ToString()
                    .Split(',')
                    .Last();
            }
        }

        async Task ConnectIfPaired(DeviceInformation device)
        {
            if(device.Name == Hand.DEVICE_NAME && device.Pairing.IsPaired && !this.State.HasFlag(DeviceSearchState.Searching))
            {
                try
                {
                    var ids = this.Hands
                        .Select(h => h.Device?.Id)
                        .ToArray();

                    bool inUse = ids.Contains(device.Id);
                    if(inUse)
                    {
                        var usedHand = this.Hands
                            .Where(h => h.Device.Id == device.Id)
                            .FirstOrDefault();
                        await usedHand.Search();
                    }
                    else
                    {
                        var freeHand = this.Hands
                            .Where((h) => h.Device == null)
                            .FirstOrDefault();

                        await freeHand?.Connect(device);
                    }

                    int handsFound = this.Hands.Count(h => h.Device != null);
                    if(handsFound == this.Hands.Count)
                    {
                        this.State |= DeviceSearchState.DevicesFound;
                    }
                }
                catch(Exception)
                {
                    this.State = DeviceSearchState.Watching;
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
            this.State &= ~DeviceSearchState.Watching;
        }
    }
}
