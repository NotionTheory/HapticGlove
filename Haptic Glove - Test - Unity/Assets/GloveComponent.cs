using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Linq;

public class GloveComponent : MonoBehaviour
{
    public GameObject[] fingers;
    public bool[] motors = new bool[5];

    TcpClient server;
    Stream stream;

    byte lastState;
    byte[] temp = new byte[] { 0, 0, 0 };
    Queue<byte> q;

    // Use this for initialization
    void Start()
    {
        q = new Queue<byte>();
        fingers = (from o in FindObjectsOfType<GameObject>()
                  where o.name.StartsWith("Finger")
                  orderby o.name
                  select o).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.server == null)
        {
            this.server = new TcpClient();
            this.server.Connect("127.0.0.1", 9001);
            this.stream = this.server.GetStream();
        }
        else if(this.server.Connected)
        {
            while(this.server.Available > 0)
            {
                var b = (byte)this.stream.ReadByte();
                if(b < byte.MaxValue)
                {
                    q.Enqueue(b);
                }
                else if(q.Count >= 2)
                {
                    while(q.Count > 2)
                    {
                        q.Dequeue();
                    }
                    int i = q.Dequeue();
                    float y = q.Dequeue() / 255f;
                    if(0 <= i && i < this.fingers.Length)
                    {
                        var f = this.fingers[i];
                        var s = f.transform.localScale;
                        s.y = y;
                        f.transform.localScale = s;
                        s = f.transform.localPosition;
                        s.y = y;
                        f.transform.localPosition = s;
                    }
                }
            }

            byte state = 0;
            for(int i = 0; i < this.motors.Length; ++i)
            {
                state <<= 1;
                if(this.motors[i])
                {
                    state |= 1;
                }
            }
            if(state != lastState)
            {
                lastState = temp[0] = state;
                this.stream.Write(temp, 0, 1);
            }
        }
        else
        {
            this.stream.Dispose();
            this.stream = null;
            this.server = null;
        }
    }
}
