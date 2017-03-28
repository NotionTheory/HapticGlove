using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBehavior : MonoBehaviour
{
    void Update()
    {
        float eulerZ = transform.eulerAngles.z;
        if(eulerZ > 60.0f)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 60.0f);
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        if(eulerZ < 0.0f)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0.0f);
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}
