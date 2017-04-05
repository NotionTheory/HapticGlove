using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleIndicator : MonoBehaviour
{

    PushButtonBehavior button;
    IndicatorLamp lamp;

    void Start()
    {
        button = GetComponentInChildren<PushButtonBehavior>();
        lamp = GetComponentInChildren<IndicatorLamp>();

        button.Clicked += Button_Clicked;
    }

    private void Button_Clicked(object sender, System.EventArgs e)
    {
        lamp.IsOn = !lamp.IsOn;
    }
}
