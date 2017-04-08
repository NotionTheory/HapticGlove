using System;
using UnityEngine;

[ExecuteInEditMode]
public class NixieTube : MonoBehaviour
{
    static string[] names = new[] {
        "Zero",
        "One",
        "Two",
        "Three",
        "Four",
        "Five",
        "Six",
        "Seven",
        "Eight",
        "Nine"
    };

    [Range(0, 9)]
    public int Value;
    int lastValue;
    public Material OnMaterial, OffMaterial;

    Renderer[] digits = new Renderer[10];

    void Start()
    {
        for(int i = 0; i < names.Length; ++i)
        {
            var child = transform.Find(names[i]);
            digits[i] = child.GetComponent<Renderer>();
        }
        Value = 0;
        lastValue = -1;
    }

    public void SetValue(int v)
    {
        Value = v;
    }

    private void Update()
    {
        if(Value != lastValue)
        {
            for(int i = 0; i < digits.Length; ++i)
            {
                digits[i].material = (i == Value) ? OnMaterial : OffMaterial;
            }
            lastValue = Value;
        }
    }
}
