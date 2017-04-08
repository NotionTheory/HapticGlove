using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class DeviceServer : MonoBehaviour
{
    static byte[] ZERO = new byte[11] { 0,0,0,0,0,0,0,0,0,0,0 };

    public float[] fingers = new float[10];
    float[] targets = new float[10];
    public float[] motors = new float[10];
    byte[] lastUpdate = new byte[11];

    const float THRESHOLD_VALUE = 0.7f;
    TcpClient socket;
    Stream stream;
    byte[] buffer = new byte[11] { 0,0,0,0,0,0,0,0,0,0,255 };

    void Start()
    {
    }

    public bool IsHandCharging(int handIndex)
    {
        for(int i = 0; i < this.fingers.Length; ++i)
        {
            if(this.fingers[i] < THRESHOLD_VALUE)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsConnected
    {
        get
        {
            try
            {
                // this temporary variable makes sure the socket doesn't get nulled by the thread before 
                // we can read the Connected property.
                var socket = this.socket;
                return socket != null && socket.Connected;
            }
            catch
            {
                return false;
            }
        }
    }

    public void ConnectToServer()
    {
        try
        {
            this.socket = new TcpClient();
            this.socket.Connect("127.0.0.1", 9001);
            this.stream = this.socket.GetStream();
        }
        catch(SocketException)
        {
            // swallow the error and try again later
            this.DisconnectFromServer();
        }
    }

    private void DisconnectFromServer()
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
    }

    void Update()
    {
        if(this.IsConnected)
        {
            this.ReadFingerState();
            this.WriteMotorState();
        }
        for(int i = 0; i < this.fingers.Length - 1; ++i)
        {
            this.fingers[i] = 0.5f * (this.fingers[i] + this.targets[i]);
        }
    }

    void ReadFingerState()
    {
        while(this.IsConnected && this.socket.Available > 0)
        {
            this.stream.Read(this.buffer, 0, Math.Min(this.socket.Available, this.buffer.Length));
            for(int i = 0; i < this.buffer.Length - 1; ++i)
            {
                this.targets[i] = this.buffer[i] / (float)byte.MaxValue;
            }

            if(this.buffer[this.buffer.Length - 1] != byte.MaxValue)
            {
                this.DisconnectFromServer();
            }
        }
    }

    void WriteMotorState()
    {
        if(this.stream != null)
        {
            for(int index = 0; index < 10; ++index)
            {
                this.buffer[index] = (byte)(this.motors[index] * 255);
            }
            this.buffer[this.buffer.Length - 1] = byte.MaxValue;
            bool changed = false;
            for(int i = 0; i < this.buffer.Length; ++i)
            {
                if(this.buffer[i] != this.lastUpdate[i])
                {
                    changed = true;
                }
                this.lastUpdate[i] = this.buffer[i];
            }
            if(changed)
            {
                try
                {
                    this.stream.Write(buffer, 0, buffer.Length);
                    this.stream.Flush();
                }
                catch
                {
                    this.DisconnectFromServer();
                }
            }
        }
    }
}
