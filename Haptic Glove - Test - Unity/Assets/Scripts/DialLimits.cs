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
        Value = (int)(euler.y * NumTicks / 360);
        euler.y = Value * 360 / NumTicks;
        Debug.Log(euler);
        this.visibleCylinder.localEulerAngles = euler;
        if(Value != lastValue && Changed != null)
        {
            Changed.Invoke(this, EventArgs.Empty);
        }
    }
}
