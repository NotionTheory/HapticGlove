using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossJetController : MonoBehaviour
{
    public CinematicEffectController cinematicEffectController;

    public GameObject bigMissileRight1;
    public GameObject bigMissileRight2;
    public GameObject smallMissileRight1;
    public GameObject smallMissileRight2;
    public GameObject bigMissileLeft1;
    public GameObject bigMissileLeft2;
    public GameObject smallMissileLeft1;
    public GameObject smallMissileLeft2;

    private int currentMissile = 0;
    private float fireMissilesInterval = 2.0f;
    private float fireMissilesTimer = 2.0f;

    public GameObject laserSpawnRight;
    public GameObject laserSpawnLeft;
    public GameObject droneLaserPrefab;
    public GameObject muzzleFlarePrefab;
    public GameObject playerHead;

    private float fireLasersInterval = 2.0f;
    private float fireLasersTimer = 2.0f;
    private bool firstFire = false;

    void Start ()
    {
		
	}
	
	void Update ()
    {
        HandleTimers();

    }

    void HandleTimers()
    {
        // Fire Laser Timer
        if (fireLasersTimer > 0)
        {
            fireLasersTimer -= Time.deltaTime;
        }
        if (fireLasersTimer < 0.5f && firstFire == false)
        {
            firstFire = true;
            //FireLasers();
        }
        if (fireLasersTimer <= 0)
        {
            //FireLasers();
            fireLasersTimer = fireLasersInterval;
            firstFire = false;
        }

        // Fire Missile Timer
        if (fireMissilesTimer > 0)
        {
            fireMissilesTimer -= Time.deltaTime;
        }
        else if (fireMissilesTimer <= 0)
        {
            ChooseMissileToFire();
            fireMissilesTimer = fireMissilesInterval;
        }
    }

    void FireLasers()
    {
        //Debug.Log("FireLasers");
        laserSpawnLeft.transform.LookAt(playerHead.transform);
        GameObject droneLaserLeft = (GameObject)Instantiate(droneLaserPrefab, laserSpawnLeft.transform.position, laserSpawnLeft.transform.rotation);
        droneLaserLeft.GetComponentInChildren<DroneLaserProjectile>().laserRigidbody.velocity = laserSpawnLeft.transform.forward * 15.0f;

        GameObject muzzleFlareLeft = (GameObject)Instantiate(muzzleFlarePrefab, laserSpawnLeft.transform.position, laserSpawnLeft.transform.rotation);
        muzzleFlareLeft.transform.parent = laserSpawnLeft.transform;
        Destroy(muzzleFlareLeft, 0.2f);

        laserSpawnRight.transform.LookAt(playerHead.transform);
        GameObject droneLaserRight = (GameObject)Instantiate(droneLaserPrefab, laserSpawnRight.transform.position, laserSpawnRight.transform.rotation);
        droneLaserRight.GetComponentInChildren<DroneLaserProjectile>().laserRigidbody.velocity = laserSpawnRight.transform.forward * 15.0f;

        GameObject muzzleFlareRight = (GameObject)Instantiate(muzzleFlarePrefab, laserSpawnRight.transform.position, laserSpawnRight.transform.rotation);
        muzzleFlareRight.transform.parent = laserSpawnRight.transform;
        Destroy(muzzleFlareRight, 0.2f);
    }

    void ChooseMissileToFire()
    {
        switch (currentMissile)
        {
            case 0:
                FireMissileAtPlayer(bigMissileRight1);
                currentMissile++;
                return;

            case 1:
                FireMissileAtPlayer(bigMissileLeft1);
                currentMissile++;
                return;

            case 2:
                FireMissileAtPlayer(smallMissileRight1);
                currentMissile++;
                return;

            case 3:
                FireMissileAtPlayer(smallMissileLeft1);
                currentMissile++;
                return;

            case 4:
                FireMissileAtPlayer(bigMissileRight2);
                currentMissile++;
                return;

            case 5:
                FireMissileAtPlayer(bigMissileLeft2);
                currentMissile++;
                return;

            case 6:
                FireMissileAtPlayer(smallMissileRight2);
                currentMissile++;
                return;

            case 7:
                FireMissileAtPlayer(smallMissileLeft2);
                currentMissile++;
                return;

            case 8:
                cinematicEffectController.currentSequenceNumber = 4;
                cinematicEffectController.jetEnemy.SetActive(false);
                return;

            default:
                return;
        }
    }

    void FireMissileAtPlayer(GameObject missile)
    {
        BossJetMissile bossJetMissile = missile.GetComponent<BossJetMissile>();
        bossJetMissile.FireMissile(playerHead.transform);
    }
}
