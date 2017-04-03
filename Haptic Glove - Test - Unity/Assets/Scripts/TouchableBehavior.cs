using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchableBehavior : MonoBehaviour
{
    TouchHaptics[] fingers = new TouchHaptics[10];

    public bool IsTouched;

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

    protected void ForFingers(Action<TouchHaptics> act)
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
        var hapticDevice = collision.gameObject.GetComponent<TouchHaptics>();
        if(hapticDevice != null)
        {
            fingers[(int)hapticDevice.finger] = hapticDevice;
            hapticDevice.PersistentMotorValue = 0.1f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var hapticDevice = collision.gameObject.GetComponent<TouchHaptics>();
        if(hapticDevice != null)
        {
            fingers[(int)hapticDevice.finger] = null;
            hapticDevice.PersistentMotorValue = 0;
        }
    }
}
