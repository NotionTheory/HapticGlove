using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSwitchLimits : MonoBehaviour
{
    public bool IsOn;

    public event EventHandler Changed;

    public float MaxThrow = 23;
    public float DeadZone = 10;
    public float Throw;

    Transform visibleSphere, controlSphere;
    Renderer tip;

    void Start()
    {
        this.visibleSphere = transform.FindChild("VisibleSphere");
        this.controlSphere = transform.FindChild("ControlSphere");
        this.tip = this.visibleSphere
            .FindChild("Cylinder")
            .FindChild("Tip")
            .GetComponent<Renderer>();
    }


    void Update()
    {
        var wasOn = IsOn;
        var euler = this.controlSphere.localEulerAngles;
        Throw = euler.z;
        if(Throw > 180)
        {
            Throw -= 360;
        }
        if(Mathf.Abs(Throw) > MaxThrow)
        {
            euler.z = Throw = Mathf.Sign(Throw) * MaxThrow;
            this.controlSphere.localEulerAngles = euler;
        }
        if(Mathf.Abs(Throw) > DeadZone)
        {
            Throw = Mathf.Sign(Throw) * MaxThrow;
            IsOn = Throw > 0;
        }
        euler.z = Throw;
        Throw /= MaxThrow;
        this.visibleSphere.localEulerAngles = euler;

        if(wasOn != IsOn && Changed != null)
        {
            Changed.Invoke(this, EventArgs.Empty);
        }
    }
}
