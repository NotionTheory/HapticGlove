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
    float z = 1.15f;
    Vector3 euler;
    public float deltaX, deltaY;
    // Use this for initialization
    void Start()
    {
        cam = FindObjectOfType<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            euler.x += Input.GetAxis("Mouse Y");
            euler.y += Input.GetAxis("Mouse X");
            cam.transform.localRotation = Quaternion.Euler(euler);
        }

        var p = Input.mousePosition;
        p.x *= f / Screen.width;
        p.y *= f / Screen.height;
        p.x -= g;
        p.y -= g;
        p *= -1;
        z -= Input.mouseScrollDelta.y * 0.005f;
        p.z = z;
        p = cam.projectionMatrix.inverse.MultiplyPoint(p);
        this.transform.localPosition = p;

        Vector3 velocity = new Vector3(),
            drive = cam.transform.localRotation * Vector3.forward,
            strafe = cam.transform.localRotation * Vector3.right;
        if(Input.GetKey(KeyCode.W))
        {
            velocity += drive;
        }
        else if(Input.GetKey(KeyCode.S)){
            velocity -= drive;
        }

        if(Input.GetKey(KeyCode.D))
        {
            velocity += strafe;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            velocity -= strafe;
        }
        velocity.y = 0;
        velocity.Normalize();
        velocity *= Time.deltaTime;
        cam.transform.localPosition += velocity;
    }
}
