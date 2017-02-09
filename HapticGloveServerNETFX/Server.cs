using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HapticGloveServer
{
    public class Server : INotifyPropertyChanged
    {
        private Dictionary<string, PropertyChangedEventArgs> propArgs;
        private List<Client> clients;
        private Thread runner;
        private bool running;

        public Server()
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.clients = new List<Client>();
            this.running = true;
            this.runner = new Thread(this.Run);
        }

        public int ClientCount
        {
            get
            {
                return this.clients.Count;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private async void OnPropertyChanged(string name)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if(!propArgs.ContainsKey(name))
                {
                    propArgs.Add(name, new PropertyChangedEventArgs(name));
                }
                this.PropertyChanged?.Invoke(this, propArgs[name]);
            });
        }

        private byte _motorState;
        public byte MotorState
        {
            get
            {
                return this._motorState;
            }
            internal set
            {
                if(this._motorState != value)
                {
                    this._motorState = value;
                    this.OnPropertyChanged("MotorState");
                }
            }
        }

        public void Start()
        {
            this.runner.Start();
        }

        public void Stop()
        {
            this.running = false;
        }

        async void Run()
        {
            var listener = new TcpListener(IPAddress.Any, 9001);
            listener.Start();
            while(this.running)
            {
                if(listener.Pending())
                {
                    var client = new Client(await listener.AcceptTcpClientAsync());
                    client.PropertyChanged += Client_PropertyChanged;
                    lock(this.clients)
                    {
                        this.clients.Add(client);
                        this.OnPropertyChanged("ClientCount");
                    }
                }

                lock(this.clients)
                {
                    for(int i = this.clients.Count - 1; i >= 0; --i)
                    {
                        var client = this.clients[i];
                        if(client.Connected)
                        {
                            client.Update();
                        }
                        else
                        {
                            this.clients.RemoveAt(i);
                            this.OnPropertyChanged("ClientCount");
                        }
                    }
                }
            }
        }

        internal void SetSensorState(int index, float value)
        {
            var i = (byte)index;
            var v = (byte)(255 * value);
            lock(this.clients)
            {
                foreach(var client in clients)
                {
                    client.SetSensorState(i, v);
                }
            }
        }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "MotorState")
            {
                var client = sender as Client;
                if(client != null)
                {
                    this.MotorState = client.MotorState;
                }
            }
        }
    }
}
