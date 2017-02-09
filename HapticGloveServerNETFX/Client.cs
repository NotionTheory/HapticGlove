using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;

namespace HapticGloveServer
{
    class Client : IDisposable, INotifyPropertyChanged
    {
        private Dictionary<string, PropertyChangedEventArgs> propArgs;
        private TcpClient client;
        private Stream stream;

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

        public Client(TcpClient client)
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.client = client;
            this.stream = client.GetStream();
        }

        static byte[] temp = new byte[] { 0, 0, 255 };

        internal void Update()
        {
            if(this.client.Available > 0)
            {
                for(int i = this.client.Available; i >= 0; --i)
                {
                    this.stream.Read(temp, 0, 1);
                }
                this.MotorState = temp[0];
            }
        }

        internal void SetSensorState(byte index, byte value)
        {
            temp[0] = index;
            temp[1] = value;
            if(this.client.Connected)
            {
                try
                {
                    this.stream.Write(temp, 0, temp.Length);
                }
                catch(Exception)
                {
                    if(this.client.Connected)
                    {
                        this.client.Close();
                    }
                }
            }
        }

        private byte _motorState;
        public byte MotorState
        {
            get
            {
                return this._motorState;
            }
            set
            {
                if(this._motorState != value)
                {
                    this._motorState = value;
                    this.OnPropertyChanged("MotorState");
                }
            }
        }

        public bool Connected
        {
            get
            {
                return this.client.Connected;
            }
        }

        public void Dispose()
        {
            this.stream.Dispose();
            this.client.Dispose();
        }
    }
}
