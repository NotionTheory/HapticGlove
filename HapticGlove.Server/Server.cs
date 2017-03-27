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

        Thread runner;
        bool running; public float[] fingers = new float[10];
        public float[] motors = new float[10];

        TcpClient socket;
        Stream stream;
        Queue<byte> q;
        byte[] buffer = new byte[11] { 0,0,0,0,0,0,0,0,0,0,255 };

        public void Update()
        {
            if(this.socket != null && this.socket.Connected)
            {
                this.GetMotorState();
            }
            else
            {
                this.DisconnectFromClient();
            }

            if(this.socket != null && this.socket.Connected)
            {
                this.SetFingerState();
            }
        }

        public bool IsConnected
        {
            get
            {
                return this.socket != null && this.socket.Connected;
            }
        }

        private void DisconnectFromClient()
        {
            if(this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
            }
            this.socket = null;
        }


        public void GetMotorState()
        {
            while(this.socket.Available > 0)
            {
                var b = (byte)this.stream.ReadByte();
                if(b < byte.MaxValue)
                {
                    q.Enqueue(b);
                }
                else if(q.Count >= 10)
                {
                    while(q.Count > 10)
                    {
                        q.Dequeue();
                    }

                    for(int index = 0; index < 10; ++index)
                    {
                        float value = q.Dequeue() / 255f;
                        if(0 <= index && index <= this.motors.Length)
                        {
                            this.motors[index] = value;
                        }
                    }
                }
                else
                {
                    // we missed a critical value
                    q.Clear();
                }
            }
        }

        private void SetFingerState()
        {
            if(this.stream != null)
            {
                for(int index = 0; index < 10; ++index)
                {
                    this.buffer[index] = (byte)(this.fingers[index] * 255);
                }
                this.stream.Write(buffer, 0, buffer.Length);
            }
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        public Server()
        {
            this.propArgs = new Dictionary<string, PropertyChangedEventArgs>();
            this.q = new Queue<byte>();
            this.running = true;
            this.runner = new Thread(this.Run);
        }

        public string Status
        {
            get
            {
                return this.IsConnected ? "client connected" : "waiting for client";
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
                    if(this.IsConnected)
                    {
                        this.DisconnectFromClient();
                    }
                    this.socket = await listener.AcceptTcpClientAsync();
                    this.stream = this.socket.GetStream();
                    this.OnPropertyChanged(nameof(Status));
                }
                
                if(this.IsConnected)
                {
                    this.Update();
                }
                else
                {
                    this.OnPropertyChanged(nameof(Status));
                    this.ClientDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void SetSensorState(int hand, int index, float value)
        {
            index += hand * 5;
            this.motors[index] = value;
        }
    }
}
