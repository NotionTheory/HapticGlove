using System;
using UnityEngine;

public class IsServerConnected : MonoBehaviour
{
    
    DeviceServer server;
    IndicatorLamp lamp;
    DateTime lastConnectionAttempt = DateTime.MinValue;
    int minWait = 5;

    void Start()
    {
        server = FindObjectOfType<DeviceServer>();
        lamp = GetComponent<IndicatorLamp>();
    }

    void Update()
    {
        if(!server.IsConnected)
        {
            var delta = DateTime.Now - lastConnectionAttempt;
            if(delta.TotalSeconds >= minWait)
            {
                server.ConnectToServer();
                lastConnectionAttempt = DateTime.Now;
                if(!server.IsConnected)
                {
                    minWait *= 2;
                }
            }
        }
        if(lamp != null)
        {
            lamp.IsOn = server.IsConnected;
        }
    }
}
