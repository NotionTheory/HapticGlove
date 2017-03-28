using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    const float f = 20;
    const float g = f/2;
    Camera cam;
    float z = 0f;
    // Use this for initialization
    void Start()
    {
        cam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        var m = cam.projectionMatrix.inverse;
        var p = Input.mousePosition;
        p.x *= f / Screen.width;
        p.y *= f / Screen.height;
        p.x -= g;
        p.y -= g;
        p *= 1;
        z -= Input.mouseScrollDelta.y * 0.05f;
        p.z = z;
        p = m.MultiplyPoint(p);
        this.transform.localPosition = p;
    }
}
