using System;
using UnityEngine;

[ExecuteInEditMode]
public class IndicatorLamp : MonoBehaviour
{

    public Material OnMaterial, OffMaterial;
    public bool IsOn;
    bool wasOn;
    Renderer rend;
    // Use this for initialization
    void Start()
    {
        IsOn = false;
        wasOn = true;
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(OnMaterial != null && OffMaterial != null)
        {
            if(IsOn != wasOn)
            {
                rend.material = IsOn ? OnMaterial : OffMaterial;
            }

            wasOn = IsOn;
        }
    }
}
