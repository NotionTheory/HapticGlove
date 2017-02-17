using UnityEngine;
using System.Collections;

public class PulseProjectile : MonoBehaviour
{
    public GameObject explosionPrefab;

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Enemy")
        {
            GameObject explosion = (GameObject)Instantiate(explosionPrefab, this.transform.position, this.transform.rotation);
        }
        Destroy(this.gameObject);
    }
}
