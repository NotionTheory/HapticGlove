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

        // Constrain the rotation, because Unity does it in World Space
        // and we want it done in local, model space.
        euler.y = 0;
        euler.z = 0;
        this.controlSphere.localEulerAngles = euler;

        Throw = euler.x;
        if(Throw > 180)
        {
            Throw -= 360;
        }
        if(Mathf.Abs(Throw) > MaxThrow)
        {
            euler.x = Throw = Mathf.Sign(Throw) * MaxThrow;
            this.controlSphere.localEulerAngles = euler;
        }
        if(Mathf.Abs(Throw) > DeadZone)
        {
            Throw = Mathf.Sign(Throw) * MaxThrow;
            IsOn = Throw > 0;
        }
        euler.x = Throw;
        Throw /= MaxThrow;
        this.visibleSphere.localEulerAngles = euler;

        if(wasOn != IsOn && Changed != null)
        {
            Changed.Invoke(this, EventArgs.Empty);
        }
    }
}
