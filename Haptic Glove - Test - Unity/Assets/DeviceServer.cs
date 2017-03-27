using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

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
    int read = 0;
    DateTime lastConnectionAttempt = DateTime.MinValue;


    public bool IsConnected
    {
        get
        {
            return this.socket != null && this.socket.Connected;
        }
    }

    public void ConnectToServer()
    {
        try
        {
            var timeSinceLast = DateTime.Now - this.lastConnectionAttempt;
            if(!this.IsConnected && timeSinceLast.TotalSeconds >= 3)
            {
                this.socket = new TcpClient();
                this.socket.Connect("127.0.0.1", 9001);
                this.stream = this.socket.GetStream();
            }
        }
        catch(SocketException)
        {
            // swallow the error and try again later
            this.DisconnectFromServer();
        }
    }

    public void DisconnectFromServer()
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
        this.lastConnectionAttempt = DateTime.Now;
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

    void Start()
    {
    }

    void Update()
    {
        if(this.IsConnected)
        {
            this.ReadFingerState();
        }
        else
        {
            this.ConnectToServer();
        }

        if(this.IsConnected)
        {
            this.WriteMotorState();
        }
    }

    private void ReadFingerState()
    {
        bool good = false;
        while(this.socket != null && this.socket.Available > 0)
        {
            good = false;
            read += this.stream.Read(this.buffer, read, Math.Min(this.socket.Available, this.buffer.Length - read));
            if(read == this.buffer.Length)
            {
                int end = -1;
                bool allAreZero = true;
                for(int i = 0; i < this.buffer.Length; ++i)
                {
                    if(this.buffer[i] > 0)
                    {
                        allAreZero = false;
                    }
                    if(this.buffer[i] == byte.MaxValue)
                    {
                        end = i;
                    }
                }
                if(allAreZero)
                {
                    this.DisconnectFromServer();
                    read = 0;
                }
                else if(end != this.buffer.Length - 1)
                {
                    for(int i = end + 1; i < this.buffer.Length; ++i)
                    {
                        this.buffer[i - end - 1] = this.buffer[i];
                    }
                    read = this.buffer.Length - end - 1;
                }
                else
                {
                    good = true;
                    read = 0;
                }
            }
        }
        if(good)
        {
            for(int i = 0; i < this.buffer.Length - 1; ++i)
            {
                this.targets[i] = this.buffer[i] / 255f;
            }
        }
        for(int i = 0; i < this.fingers.Length; ++i)
        {
            this.fingers[i] = 0.5f * (this.fingers[i] + this.targets[i]);
        }
    }

    private void WriteMotorState()
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
