using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SwarmWaypointTrigger : MonoBehaviour
{
    public CinematicEffectController cinematicEffectController;

    public bool despawnOnTrigger = true;
    public bool transitionOnTrigger = false;
    public bool disableOnTrigger = false;
    public int sequenceToTransitionTo = 0;
    [HideInInspector]
    public bool didTransition = false;

    void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Swarm")
        {
            if (despawnOnTrigger == true)
            {
                collider.gameObject.SetActive(false);
            }

            if (transitionOnTrigger == true && cinematicEffectController != null)
            {
                if (cinematicEffectController.currentSequenceNumber < sequenceToTransitionTo)
                {
                    cinematicEffectController.currentSequenceNumber = sequenceToTransitionTo;
                }
            }

            if (disableOnTrigger == true)
            {
                this.enabled = false;
            }
        }
    }
}
