using System;
using UnityEngine;

public class ToggleSwitchBehavior : TouchableBehavior
{
    public bool IsOn;
    bool wasOn;

    [Header("Range of Motion")]
    [Range(0, 25)]
    public float MaxThrow = 23;

    [Range(0, 25)]
    public float DeadZone = 10;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent TurnedOn;
    public UnityEngine.Events.UnityEvent TurnedOff;
    public UnityEngine.Events.UnityEvent Changed;

    [Header("Haptic feedback on value change")]
    [Range(0, 1)]
    public float Strength = 0.5f;
    [Range(0, 1000)]
    public int Length = 100;

    Transform visibleSphere, controlSphere;
    Renderer tip;

    void Start()
    {
        this.visibleSphere = transform.parent.FindChild("VisibleSphere");
        this.controlSphere = transform.parent.FindChild("ControlSphere");
        this.tip = this.visibleSphere
            .FindChild("Cylinder")
            .FindChild("Tip")
            .GetComponent<Renderer>();
        wasOn = !IsOn;
    }


    protected override void Update()
    {
        base.Update();
        var euler = this.controlSphere.localEulerAngles;
        if(IsOn != wasOn)
        {
            euler.x = (IsOn ? 1 : -1) * MaxThrow;
        }

        // Constrain the rotation, because Unity does it in World Space
        // and we want it done in local, model space.
        euler.y = 0;
        euler.z = 0;
        this.controlSphere.localEulerAngles = euler;

        var rotX = euler.x;
        if(rotX > 180)
        {
            rotX -= 360;
        }
        if(Mathf.Abs(rotX) > MaxThrow)
        {
            euler.x = rotX = Mathf.Sign(rotX) * MaxThrow;
            this.controlSphere.localEulerAngles = euler;
        }
        if(Mathf.Abs(rotX) > DeadZone)
        {
            rotX = Mathf.Sign(rotX) * MaxThrow;
            IsOn = rotX > 0;
        }
        euler.x = rotX;
        rotX /= MaxThrow;
        this.visibleSphere.localEulerAngles = euler;

        var color = tip.material.color;
        color.r = IsOn ? 0 : 1;
        color.g = IsTouched ? 1 : 0;
        color.b = 1;
        tip.material.color = color;

        if(IsOn != wasOn)
        {
            ForFingers((f) => f.Vibrate(Strength, Length));

            if(IsOn && TurnedOn != null)
            {
                TurnedOn.Invoke();
            }

            if(!IsOn && TurnedOff != null)
            {
                TurnedOff.Invoke();
            }

            if(Changed != null)
            {
                Changed.Invoke();
            }
        }
        wasOn = IsOn;
    }

    private void OnMouseDown()
    {
        IsOn = !IsOn;
    }
}
