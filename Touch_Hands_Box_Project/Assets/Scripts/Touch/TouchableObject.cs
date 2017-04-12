using System;
using UnityEngine;

public class TouchableObject : MonoBehaviour
{
    HapticFinger[] fingers = new HapticFinger[10];

    public bool IsTouched;
    [Range(0, 1)]
    public float MotorStrengthOnTouch = 0.5f;

    protected virtual void Update()
    {
        IsTouched = false;
        for(int i = 0; i < fingers.Length && !IsTouched; ++i)
        {
            if(fingers[i] != null)
            {
                IsTouched = true;
            }
        }
    }

    protected void ForFingers(Action<HapticFinger> act)
    {
        for(int i = 0; i < fingers.Length; ++i)
        {
            if(fingers[i] != null)
            {
                act(fingers[i]);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HapticFinger hapticDevice = FindFinger(collision);
        if(hapticDevice != null)
        {
            fingers[(int)hapticDevice.finger] = hapticDevice;
            hapticDevice.MotorValue = MotorStrengthOnTouch;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        HapticFinger hapticDevice = FindFinger(collision);
        if(hapticDevice != null)
        {
            fingers[(int)hapticDevice.finger] = null;
            hapticDevice.MotorValue = 0;
        }
    }

    private static HapticFinger FindFinger(Collision collision)
    {
        var top = collision.gameObject;
        var hapticDevice = top.GetComponentInChildren<HapticFinger>();
        while(top != null && top.transform.parent != null && hapticDevice == null)
        {
            top = top.transform.parent.gameObject;
            hapticDevice = top.GetComponentInChildren<HapticFinger>();
        }

        return hapticDevice;
    }
}
