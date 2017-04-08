using System;
using UnityEngine;

[Serializable]
public class DialValueChanged: UnityEngine.Events.UnityEvent<int>
{
}

public class DialBehavior : TouchableBehavior
{
    public int Value;
    public int NumTicks = 10;
    
    public DialValueChanged ValueChanged;

    [Header("Haptic feedback on value change")]
    [Range(0, 1)]
    public float Strength = 0.5f;
    [Range(0, 1000)]
    public int Length = 100;
    
    
    Transform visibleCylinder, controlCylinder;
    Renderer tab;
    void Start()
    {
        this.visibleCylinder = transform.parent.FindChild("VisibleCylinder");
        this.controlCylinder = transform.parent.FindChild("ControlCylinder");
        this.tab = this.visibleCylinder
            .FindChild("Tab")
            .GetComponent<Renderer>();
    }

    protected override void Update()
    {
        base.Update();
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

        tab.material.color = Color.HSVToRGB((float)Value / NumTicks, 1f, 1f);

        if(Value != lastValue && ValueChanged != null)
        {
            ForFingers((finger) => finger.Vibrate(Strength, Length));
            ValueChanged.Invoke(Value);
        }
    }
}
