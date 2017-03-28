using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialDragger : MonoBehaviour
{

    float startY, lastY;
    public NixieTube[] digits;
    float eulerX, eulerZ;
    int lastDirection, curDigit = -1;

    void Start()
    {
        this.eulerX = this.transform.localEulerAngles.x;
        this.eulerZ = this.transform.localEulerAngles.z;
    }

    int Value
    {
        get
        {
            int value = 0;
            for(int i = 0; i < this.digits.Length; ++i)
            {
                value *= 10;
                value += digits[i].Digit;
            }
            return value;
        }
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(Input.GetMouseButtonDown(0))
            {
                this.lastY = this.startY = Input.mousePosition.y;
            }
            var smallDY = Input.mousePosition.y - this.lastY;
            if(smallDY != 0)
            {
                var curDirection = Math.Sign(smallDY);
                if(curDirection != this.lastDirection)
                {
                    this.startY = 0;
                    this.curDigit = (this.curDigit + 1) % this.digits.Length;
                }
                var dy = Input.mousePosition.y - this.startY;
                this.transform.localRotation = Quaternion.Euler(this.eulerX, -dy, this.eulerZ);
                this.digits[this.curDigit].Digit = (int)(Math.Abs(dy) / 36) % 10;
                this.lastY = Input.mousePosition.y;
                this.lastDirection = curDirection;
            }
        }
    }
}
