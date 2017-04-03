using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushButtonBehavior : TouchableBehavior
{
    const float ALPHA = 0.002f;
    public bool IsBottomed;
    public bool IsTopped;

    float MinY = 0.2f, MaxY = 0.3f;

    public event EventHandler Clicked, Released;

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

    void Update()
    {
        var wasOn = IsOn;
        var position = this.transform.localPosition;
        var delta = position - lastPosition;
        delta.x = 0;
        delta.z = 0;
        if(!IsTouched && !IsTopped)
        {
            delta.y += 0.025f;
        }
        position = lastPosition + delta;
        position.y = Mathf.Min(MaxY, Mathf.Max(MinY, position.y));

        IsTopped = Mathf.Abs(MaxY - position.y) < ALPHA;
        IsBottomed = Mathf.Abs(MinY - position.y) < ALPHA;

        lastPosition = this.transform.localPosition = position;

        var color = rend.material.color;
        color.r = IsOn ? 0 : 1;
        color.g = IsTouched ? 1 : 0;
        color.b = IsTopped ? 0 : 1;
        rend.material.color = color;

        if(IsOn && !wasOn && Clicked != null)
        {
            ForFingers((f) => f.Vibrate(0.5f, 50));
            Clicked.Invoke(this, EventArgs.Empty);
        }
        else if(wasOn && !IsOn && Released != null)
        {
            ForFingers((f) => f.Vibrate(0.25f, 50));
            Released.Invoke(this, EventArgs.Empty);
        }
    }
}
