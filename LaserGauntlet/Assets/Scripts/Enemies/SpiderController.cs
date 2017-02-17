using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    public GameObject spiderParentObject;
    public Animator animationController;
    public Rigidbody rigidBody;
    public CapsuleCollider capsuleCollider;

    public GameObject smoke;

	void Start ()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        capsuleCollider = this.GetComponent<CapsuleCollider>();
	}
	
	void Update ()
    {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "PlayerProjectile" || collision.collider.tag == "PlayerBeam")
        {
            animationController.Stop();
            spiderParentObject.transform.SetParent(null);
            rigidBody.isKinematic = false;
            rigidBody.useGravity = true;
            smoke.SetActive(true);
            Destroy(spiderParentObject, 10.0f);
            //capsuleCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "PlayerProjectile" || collider.tag == "PlayerBeam")
        {
            animationController.Stop();
            spiderParentObject.transform.SetParent(null);
            rigidBody.isKinematic = false;
            rigidBody.useGravity = true;
            smoke.SetActive(true);
            Destroy(spiderParentObject, 10.0f);
            //capsuleCollider.enabled = false;
        }
    }
}
