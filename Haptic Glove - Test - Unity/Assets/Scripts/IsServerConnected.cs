using System;
using UnityEngine;

public class IsServerConnected : MonoBehaviour
{
    DeviceServer server;
    Material mat;
    DateTime lastConnectionAttempt = DateTime.MinValue;
    int minWait = 5;

    void Start()
    {
        this.server = FindObjectOfType<DeviceServer>();
        this.mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if(!this.server.IsConnected)
        {
            var delta = DateTime.Now - lastConnectionAttempt;
            if(delta.TotalSeconds >= minWait)
            {
                this.server.ConnectToServer();
                lastConnectionAttempt = DateTime.Now;
                if(!this.server.IsConnected)
                {
                    minWait *= 2;
                }
            }
        }
        this.mat.SetColor("_EmissionColor", this.server.IsConnected ? Color.green : Color.red);
    }
}
