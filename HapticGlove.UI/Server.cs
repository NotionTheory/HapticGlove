using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NotionTheory.HapticGlove
{
    public class Server : INotifyPropertyChanged, IDisposable
    {
        static byte[] ZERO = new byte[11] { 0,0,0,0,0,0,0,0,0,0,0 };
        static int[] mapSensors = {
            0, 1, 2, 3, 4,
            5, 6, 7, 8, 9,
        };
        static int[] mapMotors = {
            4, 3, 2, 1, 0,
            5, 6, 7, 8, 9,
        };

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

        public event EventHandler ClientDisconnected;
        public float[] fingers = new float[10];
        public float[] motors = new float[10];
        byte[] lastFingers = new byte[11];
        byte[] lastMotors = new byte[11];
        
        TcpClient socket;
        TcpListener listener;
        Stream stream;
        byte[] buffer = new byte[11] { 0,0,0,0,0,0,0,0,0,0,255 };
        bool wasConnected;

        public Server()
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.wasConnected = false;
            this.listener = new TcpListener(IPAddress.Any, 9001);
            this.listener.Start();
        }

        public async void Update()
        {
            if(this.listener.Pending())
            {
                if(this.IsConnected)
                {
                    this.DisconnectFromClient();
                }
                this.socket = await this.listener.AcceptTcpClientAsync();
                this.stream = this.socket.GetStream();
                this.OnPropertyChanged(nameof(Status));
            }

            if(this.IsConnected)
            {
                this.ReadMotorState();
                this.WriteFingerState();
            }
            else if(this.wasConnected)
            {
                this.DisconnectFromClient();
            }

            this.wasConnected = this.IsConnected;
        }

        public bool IsConnected
        {
            get
            {
                return this.socket != null && this.socket.Connected;
            }
        }

        public void DisconnectFromClient()
        {
            if(this.IsConnected)
            {
                this.stream.Write(ZERO, 0, ZERO.Length);
                this.stream.Flush();
                this.socket.Close();
                this.socket = null;
            }
            if(this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
            }
            this.OnPropertyChanged(nameof(Status));
            this.ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }


        public void ReadMotorState()
        {

            while(this.IsConnected && this.socket.Available > 0)
            {
                this.stream.Read(this.buffer, 0, Math.Min(this.socket.Available, this.buffer.Length));
                bool changed = false;
                for(int i = 0; i < this.buffer.Length - 1; ++i)
                {
                    if(this.buffer[i] != this.lastMotors[i])
                    {
                        changed = true;
                    }
                    this.lastMotors[i] = this.buffer[i];
                    this.motors[mapMotors[i]] = this.buffer[i] / 255f;
                }
                if(changed)
                {
                    this.OnPropertyChanged("Motors");
                }

                if(this.buffer[this.buffer.Length - 1] != byte.MaxValue)
                {
                    this.DisconnectFromClient();
                }
            }
        }

        public void SetFinger(Finger finger)
        {
            if(finger != null)
            {
                int hand = (int)finger.Hand.Side;
                int index = finger.Index + hand * 5;
                this.fingers[index] = finger.SensorValue;
            }
        }

        private void WriteFingerState()
        {
            if(this.IsConnected && this.stream != null)
            {
                for(int index = 0; index < 10; ++index)
                {
                    this.buffer[mapSensors[index]] = (byte)(this.fingers[index] * 255);
                }
                this.buffer[this.buffer.Length - 1] = byte.MaxValue;

                try
                {
                    this.stream.Write(buffer, 0, buffer.Length);
                    this.stream.Flush();
                }
                catch
                {
                    this.DisconnectFromClient();
                }
            }
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        public string Status
        {
            get
            {
                return this.IsConnected ? "client connected" : "waiting for client";
            }
        }
    }
}
