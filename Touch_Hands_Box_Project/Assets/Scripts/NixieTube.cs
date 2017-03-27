using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Renderer[] digits = new Renderer[10];
    int _digit;
    public Material on, off;

    void Start()
    {
        for(int i = 0; i < names.Length; ++i)
        {
            var child = this.transform.Find(names[i]);
            this.digits[i] = child.GetComponent<Renderer>();
        }
        this.Digit = 0;
    }

    public int Digit
    {
        get
        {
            return this._digit;
        }
        set
        {
            this._digit = value;
            for(int i = 0; i < digits.Length; ++i)
            {
                digits[i].material = (i == this._digit) ? this.on : this.off;
            }
        }
    }

    private void Update()
    {
    }
}
