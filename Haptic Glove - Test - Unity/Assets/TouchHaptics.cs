using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHaptics : MonoBehaviour
{
    private DeviceServer server;
    private Renderer rend;
    private Color c;
    public int fingerIndex;

    void Start()
    {
        int finger = this.transform.GetSiblingIndex();
        int hand = this.transform.parent.GetSiblingIndex() - 2;
        this.fingerIndex = hand * 5 + finger;
        this.server = FindObjectOfType<DeviceServer>();
        this.rend = GetComponent<Renderer>();
        this.c = this.rend.material.color;
    }

    void Update()
    {
        var y = this.server.fingers[this.fingerIndex] * 0.9f + 0.1f;
        var s = this.transform.localScale;
        s.y = y;
        this.transform.localScale = s;
        s = this.transform.localPosition;
        s.y = y;
        this.transform.localPosition = s;
        if(this.server.motors[this.fingerIndex] > 0.5f)
        {
            this.c.r = 0;
            this.c.g = 1;
            this.c.b = 0;
            this.rend.material.color = this.c;
        }
        else
        {
            this.c.r = 1;
            this.c.g = 0;
            this.c.b = 0;
            this.rend.material.color = this.c;
        }
    }

    void OnMouseDown()
    {
        this.server.motors[this.fingerIndex] = 1;
    }

    void OnMouseUp()
    {
        this.server.motors[this.fingerIndex] = 0;
    }
}
