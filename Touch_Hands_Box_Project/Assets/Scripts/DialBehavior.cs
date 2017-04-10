using System;
using UnityEngine;

[Serializable]
public class DialValueChanged : UnityEngine.Events.UnityEvent<int>
{
}

public class DialBehavior : TouchableBehavior
{
    [Range(0, 9)]
    public int Value;
    int lastValue;
    public int NumTicks = 10;

    public DialValueChanged OnValueChanged;

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
        lastValue = -1;
    }

    protected override void Update()
    {
        base.Update();
        var euler = this.controlCylinder.localEulerAngles;
        var recalc = true;
        if(Value != lastValue)
        {
            Value = Value % 10;
            euler.y = Value * 360 / NumTicks;
            recalc = false;
        }

        // Constrain the rotation, because Unity does it in World Space
        // and we want it done in local, model space.
        euler.x = 0;
        euler.z = 0;
        this.controlCylinder.localEulerAngles = euler;
        
        if(recalc)
        {
            // Calculate which digit we're pointing at.
            Value = ((int)Mathf.Round(euler.y * NumTicks / 360)) % NumTicks;
        }

        // Chunk the visible dial over there.
        euler.y = Value * 360 / NumTicks;
        this.visibleCylinder.localEulerAngles = euler;

        tab.material.color = Color.HSVToRGB((float)Value / NumTicks, 1f, 1f);

        if(Value != lastValue)
        {
            ForFingers((finger) => finger.Vibrate(Strength, Length));
            if(OnValueChanged != null)
            {
                OnValueChanged.Invoke(Value);
            }
        }
        lastValue = Value;
    }

    private void OnMouseDown()
    {
        Advance();
    }

    public void Advance()
    {
        ++Value;
    }
}
