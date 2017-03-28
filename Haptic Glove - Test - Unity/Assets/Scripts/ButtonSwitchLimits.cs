using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitchLimits : MonoBehaviour
{

    public bool IsBottomed;
    public bool IsTouched;
    public bool IsTopped;
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
            delta.y += 0.01f;
        }
        this.transform.localPosition = position = lastPosition + delta;

        lastPosition = position;

        var color = rend.material.color;
        color.r = IsOn ? 0 : 1;
        color.g = IsTouched ? 1 : 0;
        color.b = IsTopped ? 0 : 1;
        rend.material.color = color;

        if(IsOn && !wasOn && Clicked != null)
        {
            Clicked.Invoke(this, EventArgs.Empty);
        }
        else if(wasOn && !IsOn && Released != null)
        {
            Released.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var obj = collision.collider.name;
        if(obj == "Limit")
        {
            IsBottomed = false;
            IsTopped = true;
        }
        else if(obj == "Enclosure")
        {
            IsBottomed = true;
        }
        else
        {
            IsTouched = true;
            IsTopped = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var obj = collision.collider.name;
        if(obj != "Limit" && obj != "Enclosure")
        {
            IsTouched = false;
        }
    }
}
