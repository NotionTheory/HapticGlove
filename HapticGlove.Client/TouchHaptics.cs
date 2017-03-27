using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHaptics : MonoBehaviour
{
    DeviceServer server;
    public int fingerIndex = 0;
    Animation anim;
    SphereCollider here;

    private void Start()
    {
        this.here = this.GetComponent<SphereCollider>();
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
        try
        {
            var state = this.anim[this.anim.name + "Curl"];
            if(state != null)
            {
                state.normalizedTime = 1 - (float)Math.Pow(this.server.fingers[this.fingerIndex], 0.5);
                state.speed = 0;
                this.anim.Play();
            }
        }
        catch
        {
            Debug.LogErrorFormat("i{0}", this.fingerIndex);
        }
    }

    //0 = RThumb, 1 = RIndex, 2 = RMiddle, 3 = RRing, 4 = RPinky, 5 = LThumb, 6 = LIndex, 7 = LMiddle, 8 = LRing, 9 = LPinky


    float scaleFactor = 10f;
    float powerFactor = 0.5f;
    void OnTriggerStay(Collider other)
    {
        bool isWater = other.gameObject.CompareTag("water"),
             isSolid = other.gameObject.CompareTag("solid");
        if(isSolid || isWater)
        {
            float a = Vector3.Distance(other.transform.position, transform.position);
            float b = other.bounds.extents.magnitude + this.here.radius - a;
            float c = this.scaleFactor * Math.Abs(b);
            float d = (float)Math.Pow(c, this.powerFactor);
            float v = Math.Max(0, Math.Min(1, d));
            if(isWater)
            {
                v *= 0.5f;
            }

            this.server.motors[this.fingerIndex] = v;
        }
    }

    void OnTriggerExit(Collider other)
    {
        bool isWater = other.gameObject.CompareTag("water"),
             isSolid = other.gameObject.CompareTag("solid");
        if(isSolid || isWater)
        {
            this.server.motors[this.fingerIndex] = 0.0f;
        }
    }
}
