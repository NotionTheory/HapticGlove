using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossJetMissile : MonoBehaviour
{
    public GameObject missileHolder;
    public Light missileLight;
    public GameObject explosion;
    public Rigidbody missileRigidbody;

    Transform missileTarget;
    
    public bool isStarting = false;
    public bool isFiring = false;

    private float missileSpeed = 25.0f;

    private float missileFireInterval = 1.0f;
    private float missileFireTimer = 0.0f;

	void Start ()
    {
        missileRigidbody.isKinematic = true;
	}
	
	void Update ()
    {

	}

    public void ActivateMissileLight()
    {
        missileLight.gameObject.SetActive(true);
        missileLight.intensity = 2.0f;
    }

    public void FireMissile(Transform target)
    {
        isStarting = true;
        ActivateMissileLight();
        missileTarget = target;
        missileHolder.transform.LookAt(missileTarget);
        missileRigidbody.isKinematic = false;
        missileRigidbody.velocity = missileHolder.transform.forward * missileSpeed;
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Missile hits not player
        if (collider.tag == "Untagged")
        {
            //AudioSource.PlayClipAtPoint(laserImpactClip, this.transform.position);
            explosion.SetActive(true);
            missileRigidbody.isKinematic = true;
            this.GetComponent<MeshRenderer>().enabled = false;
            missileLight.intensity = 0.0f;
            Destroy(missileHolder, 2.0f);
        }

        // Missile hits player
        if (collider.tag == "MainCamera")
        {
            //AudioSource.PlayClipAtPoint(laserImpactClip, this.transform.position);
            explosion.SetActive(true);
            missileRigidbody.isKinematic = true;
            this.GetComponent<MeshRenderer>().enabled = false;
            missileLight.intensity = 0.0f;
            Destroy(missileHolder, 2.0f);
            // INDICATE PLAYER WAS HIT
            // TAKE AWAY SCORE FROM PLAYER
            collider.GetComponent<ScoreController>().SubtractScore(100);
        }

        // Player shoots missile
        if ((collider.tag == "PlayerProjectile" || collider.tag == "PlayerBeam") && isStarting == true)
        {
            //AudioSource.PlayClipAtPoint(laserImpactClip, this.transform.position);
            explosion.SetActive(true);
            missileRigidbody.isKinematic = true;
            this.GetComponent<MeshRenderer>().enabled = false;
            missileLight.intensity = 0.0f;
            Destroy(missileHolder, 2.0f);
        }
    }

}
