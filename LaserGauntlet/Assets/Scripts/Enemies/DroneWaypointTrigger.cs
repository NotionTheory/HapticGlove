using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class DroneWaypointTrigger : MonoBehaviour
{
    public bool shootOnTrigger = true;
    public bool playAlertAudio = false;
    public bool transitionOnTrigger = false;
    public bool countAsExploded = false;
    public bool disableOnTrigger = false;
    public int sequenceToTransitionTo = 0;
    [HideInInspector]
    public bool didTransition = false;

    public CinematicEffectController cinematicEffectController;

	void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.GetComponent<DroneController>())
        {
            DroneController droneController = collider.GetComponent<DroneController>();
            if (shootOnTrigger == true)
            {
                droneController.FireAtPlayer();
            }

            if(playAlertAudio == true)
            {
                droneController.bodyAudioSource.Play();
            }

            if(transitionOnTrigger == true && cinematicEffectController != null)
            {
                droneController.gameObject.SetActive(false);
                if (cinematicEffectController.currentSequenceNumber < sequenceToTransitionTo)
                {
                    cinematicEffectController.currentSequenceNumber = sequenceToTransitionTo;
                }
            }

            if(countAsExploded == true)
            {
                droneController.exploded = true;
                droneController.gameObject.SetActive(false);
                //Debug.Log(droneController.gameObject.name + " count as exploded");
            }

            if (disableOnTrigger == true)
            {
                this.enabled = false;
            }
        }
    }
}
