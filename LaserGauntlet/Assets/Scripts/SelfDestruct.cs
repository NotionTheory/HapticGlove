using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour
{
    public float lifeTime = 10.0f;
    private float timer = 0.0f;

    public bool isExplosion = false;

	void Start ()
    {
	}
	
	void Update ()
    {
        if(timer < lifeTime)
        {
            timer += Time.deltaTime;

            if(isExplosion == true && this.gameObject.tag == "PlayerProjectile" && this.GetComponent<SphereCollider>() == true && timer > 0.75f)
            {
                this.GetComponent<SphereCollider>().enabled = false;
            }
        } else
        {
            Destroy(this.gameObject);
        }
	}
}
