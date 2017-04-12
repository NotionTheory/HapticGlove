using System;
using System.Collections.Generic;
using UnityEngine;

public class HapticFinger : MonoBehaviour
{
    public enum Fingers {
        LeftThumb,
        LeftIndex,
        LeftMiddle,
        LeftRing,
        LeftPinky,
        RightThumb,
        RightIndex,
        RightMiddle,
        RightRing,
        RightPinky
    }

    public Fingers finger = 0;

    DeviceServer server;
    Animation anim;

    private void Start()
    {
        server = FindObjectOfType<DeviceServer>();
        var head = transform;
        while(anim == null && head != null)
        {
            anim = head.GetComponent<Animation>();
            head = head.transform.parent;
        }
    }

    public float MotorValue
    {
        set
        {
            int index = (int)finger;
            if(server != null && 0 <= index && index < server.motors.Length)
            {
                server.motors[index] = Mathf.Max(0f, Mathf.Min(1f, value));
            }
        }
        get
        {
            int index = (int)finger;
            if(server != null && 0 <= index && index < server.motors.Length)
            {
                return server.motors[index];
            }
            return -1;
        }
    }

    public float FingerValue
    {
        get
        {
            int index = (int)finger;
            if(server != null && 0 <= index && index < server.fingers.Length)
            {
                return 1f - Mathf.Pow(server.fingers[index], 0.5f);
            }
            return 0;
        }
    }

    private void Update()
    {
        UpdateFingerValue();
    }

    private void UpdateFingerValue()
    {
        if(anim != null)
        {
            var state = anim[anim.name + "Curl"];
            if(state != null)
            {
                state.normalizedTime = FingerValue;
                state.speed = 0;
                anim.Play();
            }
        }
    }
}
