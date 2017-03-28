using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialLimits : MonoBehaviour
{
    public event EventHandler Changed;
    public int Value;
    public int NumTicks = 10;
    Transform visibleCylinder, controlCylinder;
    Renderer tab;
    void Start()
    {
        this.visibleCylinder = transform.FindChild("VisibleCylinder");
        this.controlCylinder = transform.FindChild("ControlCylinder");
        this.tab = this.visibleCylinder
            .FindChild("Tab")
            .GetComponent<Renderer>();
    }

    void Update()
    {
        var lastValue = Value;
        var euler = this.controlCylinder.localEulerAngles;

        // Constrain the rotation, because Unity does it in World Space
        // and we want it done in local, model space.
        euler.x = 0;
        euler.z = 0;
        this.controlCylinder.localEulerAngles = euler;

        // Calculate which digit we're pointing at.
        Value = (int)(euler.y * NumTicks / 360);

        // Chunk the visible dial over there.
        euler.y = Value * 360 / NumTicks;
        this.visibleCylinder.localEulerAngles = euler;

        if(Value != lastValue && Changed != null)
        {
            Changed.Invoke(this, EventArgs.Empty);
        }
    }
}
