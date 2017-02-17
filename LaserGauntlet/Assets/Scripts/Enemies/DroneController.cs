using UnityEngine;
using System.Collections;
using SWS;

public class DroneController : MonoBehaviour
{
    public GameObject droneExplosion;
    public Rigidbody bodyRigidbody;
    public Rigidbody leftWingRigidbody;
    public Rigidbody rightWingRigidbody;

    public GameObject droneLaserPrefab;
    public GameObject muzzleFlarePrefab;
    public GameObject droneLaserSpawn;

    public AudioSource leftWingAudioSource;
    public AudioSource rightWingAudioSource;
    public AudioSource bodyAudioSource;

    public splineMove moveAlongSpline;

    public GameObject playerHead;

    [HideInInspector]
    public bool exploded = false;

	void Start ()
    {

	}
	
	void Update ()
    {
	
	}

    public bool CanSeePlayer()
    {
        // Raycast and return if player can be seen
        return false;
    }

    public void FireAtPlayer()
    {
        droneLaserSpawn.transform.LookAt(playerHead.transform);
        GameObject droneLaser = (GameObject)Instantiate(droneLaserPrefab, droneLaserSpawn.transform.position, droneLaserSpawn.transform.rotation);
        GameObject muzzleFlare = (GameObject)Instantiate(muzzleFlarePrefab, droneLaserSpawn.transform.position, droneLaserSpawn.transform.rotation);
        muzzleFlare.transform.parent = droneLaserSpawn.transform;
        droneLaser.GetComponentInChildren<DroneLaserProjectile>().laserRigidbody.velocity = droneLaserSpawn.transform.forward * 20.0f;
        Destroy(muzzleFlare, 0.2f);
    }

    public void ExplodeDrone()
    {
        moveAlongSpline.Stop();
        exploded = true;
        if (droneExplosion != null)
        {
            droneExplosion.SetActive(true);
        }
        bodyRigidbody.isKinematic = false;
        leftWingRigidbody.isKinematic = false;
        rightWingRigidbody.isKinematic = false;
        bodyRigidbody.useGravity = true;
        leftWingRigidbody.useGravity = true;
        rightWingRigidbody.useGravity = true;
        leftWingAudioSource.Stop();
        rightWingAudioSource.Stop();
    }
}
