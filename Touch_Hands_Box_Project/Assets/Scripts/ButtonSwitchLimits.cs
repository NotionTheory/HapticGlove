using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitchLimits : MonoBehaviour
{

    public bool IsBottomed;
    public bool IsTouched;
    public event EventHandler Clicked, Released;

    Renderer rend;
    
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
        var color = rend.material.color;
        color.r = IsOn ? 0 : 1;
        color.g = IsTouched ? 1 : 0;
        color.b = 0;
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
        }
        else if(obj == "Enclosure")
        {
            IsBottomed = true;
        }
        else
        {
            IsTouched = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var obj = collision.collider.name;
        if(obj == "Enclosure")
        {
            IsBottomed = false;
        }
        else if(obj != "Limit")
        {
            IsTouched = false;
        }
    }
}
