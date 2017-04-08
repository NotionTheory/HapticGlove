using System;
using UnityEngine;

public class PushButtonBehavior : TouchableBehavior
{
    const float ALPHA = 0.002f;
    public bool IsBottomed;
    public bool IsTopped;

    public event EventHandler Clicked, Released;

    [Range(0, 0.1f)]
    public float SpringBack = 0.025f;
    
    public float MinimumPosition = 0.2f;

    public float MaximumPosition = 0.3f;

    [Header("Haptic feedback")]
    [Range(0, 1)]
    public float StrengthOnPress = 0.5f;
    [Range(0, 1000)]
    public int LengthOnPress = 100;

    [Range(0, 1)]
    public float StrengthOnRelease = 0.5f;
    [Range(0, 1000)]
    public int LengthOnRelease = 100;

    Renderer rend;
    Vector3 lastPosition;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public bool IsOn
    {
        get
        {
            return IsTouched && IsBottomed;
        }
    }

    protected override void Update()
    {
        base.Update();
        var wasOn = IsOn;
        var position = this.transform.localPosition;
        var delta = position - lastPosition;
        delta.x = 0;
        delta.z = 0;
        if(!IsTouched && !IsTopped)
        {
            delta.y += SpringBack;
        }
        position = lastPosition + delta;
        position.y = Mathf.Min(MaximumPosition, Mathf.Max(MinimumPosition, position.y));

        IsTopped = Mathf.Abs(MaximumPosition - position.y) < ALPHA;
        IsBottomed = Mathf.Abs(MinimumPosition - position.y) < ALPHA;

        lastPosition = this.transform.localPosition = position;

        var color = rend.material.color;
        color.r = IsOn ? 0 : 1;
        color.g = IsTouched ? 1 : 0;
        color.b = IsTopped ? 0 : 1;
        rend.material.color = color;

        if(IsOn && !wasOn && Clicked != null)
        {
            ForFingers((f) => f.Vibrate(StrengthOnPress, LengthOnPress));
            Clicked.Invoke(this, EventArgs.Empty);
        }
        else if(wasOn && !IsOn && Released != null)
        {
            ForFingers((f) => f.Vibrate(StrengthOnRelease, LengthOnRelease));
            Released.Invoke(this, EventArgs.Empty);
        }
    }
}
