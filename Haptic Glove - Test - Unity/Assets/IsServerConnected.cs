using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsServerConnected : MonoBehaviour
{
    private DeviceServer glove;
    private Renderer rend;
    private Color c;

    void Start()
    {
        this.glove = FindObjectOfType<DeviceServer>();
        this.rend = GetComponent<Renderer>();
        this.c = this.rend.material.color;
    }

    private void OnMouseUp()
    {
        if(this.glove.IsConnected)
        {
            this.glove.DisconnectFromServer();
        }
        else
        {
            this.glove.ConnectToServer();
        }
    }

    void Update()
    {
        if(this.glove.IsConnected)
        {
            this.c.r = 0;
            this.c.g = 1;
            this.c.b = 0;
        }
        else
        {
            this.c.r = 1;
            this.c.g = 0;
            this.c.b = 0;
        }
        this.rend.material.color = this.c;
    }
}
