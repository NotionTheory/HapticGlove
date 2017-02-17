using UnityEngine;
using System.Collections;
using Exploder.Utils;
using Exploder;

public class DroneDamageController : MonoBehaviour
{
    public DroneController droneController;
    private ExploderObject exploder;

	void Start ()
    {
        //
        // access exploder from singleton
        //
        exploder = Exploder.Utils.ExploderSingleton.ExploderInstance;

        CrackObject();
        //exploder.CrackObject(this.gameObject);
    }
	
	void Update ()
    {
	
	}

    void CrackObject()
    {
        exploder.CrackObject(this.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "PlayerProjectile" || collision.collider.tag == "PlayerBeam")
        {
            exploder.ExplodeCracked(this.gameObject);
            droneController.ExplodeDrone();
            //ExploderSingleton.ExploderInstance.ExplodeObject(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "PlayerProjectile" || collider.tag == "PlayerBeam")
        {
            droneController.ExplodeDrone();
            exploder.ExplodeCracked(this.gameObject);
            //ExploderSingleton.ExploderInstance.ExplodeObject(this.gameObject);
        }
    }
}
