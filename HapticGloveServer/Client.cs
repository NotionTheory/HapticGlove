using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

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
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

        static byte[] temp = new byte[] { 0,0, 255 };

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
            this.stream.Write(temp, 0, temp.Length);
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
