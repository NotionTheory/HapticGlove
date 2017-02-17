using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Linq;

public class GloveComponent : MonoBehaviour
{

    public Animation[] fingers;
    public bool[] motors = new bool[5];

    private float[] lastFingerValues;

    private float thresholdValue = 0.7f;

    public GauntletController gauntletController;

    TcpClient server;
    Stream stream;

    byte lastState;
    byte[] temp = new byte[] { 0, 0, 0 };
    Queue<byte> q;

    // Use this for initialization
    void Start()
    {
        q = new Queue<byte>();

        lastFingerValues = new float[5];

        fingers[0].Stop();
        fingers[1].Stop();
        fingers[2].Stop();
        fingers[3].Stop();
        fingers[4].Stop();

    }

    // Update is called once per frame
    void Update()
    {
        if(this.server == null)
        {
            ConnectToServer();
        }
        else if(this.server.Connected)
        {
            UpdateServer();
        }
        else
        {
            DisconnectFromServer();
        }
    }

    private void DisconnectFromServer()
    {
        if(this.stream != null)
        {
            this.stream.Dispose();
            this.stream = null;
        }
        this.server = null;
    }

    private void UpdateServer()
    {
        while (this.server.Available > 0)
        {
            var b = (byte)this.stream.ReadByte();
            if (b < byte.MaxValue)
            {
                q.Enqueue(b);
            }
            else if (q.Count >= 2)
            {
                while (q.Count > 2)
                {
                    q.Dequeue();
                }
                int i = q.Dequeue();
                float y = q.Dequeue() / 255f;
                UpdateFinger(i, y);
            }
        }

        byte state = 0;
        for (int i = 0; i < this.motors.Length; ++i)
        {
            state <<= 1;
            if (this.motors[i])
            {
                state |= 1;
            }
        }
        if (state != lastState)
        {
            lastState = temp[0] = state;
            this.stream.Write(temp, 0, 1);
        }
    }

    private void ConnectToServer()
    {
        this.server = new TcpClient();
        this.server.Connect("127.0.0.1", 9001);
        this.stream = this.server.GetStream();
    }

    private string[] FingerAnimationNames = new string[] {
        "ThumbCurl",
        "IndexCurl",
        "MiddleCurl",
        "RingCurl",
        "PinkyCurl"
    };

    private void UpdateFinger(int index, float value)
    {
        if (0 <= index && index < this.fingers.Length)
        {
            var f = this.fingers[index];
            lastFingerValues[index] = value;
            var animationName = FingerAnimationNames[index];
            var animation = f[animationName];
            animation.normalizedTime = value;
            animation.speed = 0;
            f.Play();

            gauntletController.charging = true;
            for(int i = 0; i < this.fingers.Length && gauntletController.charging; ++i)
            {
                gauntletController.charging = lastFingerValues[i] > thresholdValue;
            }
        }
    }
}
