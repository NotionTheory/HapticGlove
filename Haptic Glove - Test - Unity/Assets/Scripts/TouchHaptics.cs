using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHaptics : MonoBehaviour
{
    private class HapticFrame
    {
        public float strength;
        public float duration;

        public HapticFrame(float strength, float duration)
        {
            this.strength = strength;
            this.duration = duration;
        }
    }

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

    [Range(0f, 1f)]
    public float PersistentMotorValue = 0f;
    
    public Fingers finger = 0;

    DeviceServer server;
    Animation anim;
    Queue<HapticFrame> sequence = new Queue<HapticFrame>();

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
            server.motors[(int)finger] = Mathf.Max(0f, Mathf.Min(1f, value));
        }
    }

    public float FingerValue
    {
        get
        {
            return 1f - Mathf.Pow(server.fingers[(int)finger], 0.5f);
        }
    }

    public void Vibrate(float strength, float duration)
    {
        sequence.Enqueue(new HapticFrame(strength, duration));
    }

    private void Update()
    {
        UpdateFingerValue();
        UpdateMotorValue();
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

    private void UpdateMotorValue()
    {
        if(sequence.Count == 0)
        {
            MotorValue = PersistentMotorValue;
        }
        else
        {
            var cur = sequence.Peek();
            MotorValue = cur.strength;
            cur.duration -= Time.deltaTime;
            if(cur.duration <= 0)
            {
                sequence.Dequeue();
            }
        }
    }
}
