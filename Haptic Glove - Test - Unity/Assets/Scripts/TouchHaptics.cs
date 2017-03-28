using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHaptics : MonoBehaviour
{
    DeviceServer server;
    public int fingerIndex = 0;
    Animation anim;

    private void Start()
    {
        this.server = FindObjectOfType<DeviceServer>();
        var head = this.transform;
        while(this.anim == null && head != null)
        {
            this.anim = head.GetComponent<Animation>();
            head = head.transform.parent;
        }
    }

    private void Update()
    {
        if(this.anim != null)
        {
            var state = this.anim[this.anim.name + "Curl"];
            if(state != null)
            {
                state.normalizedTime = 1 - (float)Math.Pow(this.server.fingers[this.fingerIndex], 0.5);
                state.speed = 0;
                this.anim.Play();
            }
        }
    }

    //0 = RThumb, 1 = RIndex, 2 = RMiddle, 3 = RRing, 4 = RPinky, 5 = LThumb, 6 = LIndex, 7 = LMiddle, 8 = LRing, 9 = LPinky

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.collider.name);
    }
}
