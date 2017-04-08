using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.ImageEffects;

public class MouseFollow : MonoBehaviour
{
    const float f = 2;
    const float g = f/2;
    const float s = 0.5f;
    Camera cam;
    float z = 1.15f, lastX, lastY;
    Vector3 euler;
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
        var x = p.x;
        var y = p.y;
        p.x *= f / Screen.width;
        p.y *= f / Screen.height;
        p.x -= g;
        p.y -= g;
        p *= -1;
        z -= Input.mouseScrollDelta.y * 0.005f;
        p.z = z;
        p = m.MultiplyPoint(p);
        this.transform.localPosition = p;
        if(Input.GetMouseButton(0))
        {
            var deltaX = s * (x - lastX);
            var deltaY = s * (y - lastY);
            euler.x += deltaY;
            euler.y -= deltaX;
            cam.transform.localRotation = Quaternion.Euler(euler);
        }
        lastX = x;
        lastY = y;
    }
}
