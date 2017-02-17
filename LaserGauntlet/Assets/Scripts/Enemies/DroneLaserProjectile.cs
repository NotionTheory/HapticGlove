using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneLaserProjectile : MonoBehaviour
{
    public GameObject laserParentGO;
    public Rigidbody laserRigidbody;
    public AudioClip laserImpactClip;

	void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    private void OnTriggerEnter(Collider collider)
    {
        // Laser hits not player
        if(collider.tag == "Untagged")
        {
            AudioSource.PlayClipAtPoint(laserImpactClip, this.transform.position);
            Destroy(laserParentGO);
        }

        // Laser hits player
        if(collider.tag == "MainCamera")
        {
            AudioSource.PlayClipAtPoint(laserImpactClip, this.transform.position);
            Destroy(laserParentGO);
            // INDICATE PLAYER WAS HIT
            // TAKE AWAY SCORE FROM PLAYER
            collider.GetComponent<ScoreController>().SubtractScore(100);
        }
    }
}
