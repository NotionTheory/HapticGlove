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
    public UnityEngine.Events.UnityEvent OnCarry;

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

    public void AdvanceTo(int v)
    {
        Value = (Value + 1) % v;
        if(Value == 0 && OnCarry != null)
        {
            OnCarry.Invoke();
        }
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
