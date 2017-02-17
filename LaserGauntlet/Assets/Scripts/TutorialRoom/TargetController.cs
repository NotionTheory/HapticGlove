using UnityEngine;
using System.Collections;
using Exploder.Utils;
using Exploder;

public class TargetController : MonoBehaviour
{
    public TutorialRoomController tutorialRoomController;
    public GameObject bullsEye;

    private ExploderObject exploder;

    public AudioClip[] breakAudioClips;

	void Start ()
    {
        //
        // access exploder from singleton
        //
        exploder = Exploder.Utils.ExploderSingleton.ExploderInstance;

        //exploder.CrackObject(this.gameObject);
        CrackTarget();
	}
	
	void Update ()
    {

	}

    public void CrackTarget()
    {
        exploder.CrackObject(this.gameObject);
    }

    public void DestroyTarget()
    {
        //ExploderSingleton.ExploderInstance.ExplodeObject(this.gameObject);
        int clipToPlay = Random.Range(1, breakAudioClips.Length);
        AudioSource.PlayClipAtPoint(breakAudioClips[clipToPlay], this.transform.position);
        bullsEye.transform.parent = null;
        bullsEye.SetActive(false);
        this.gameObject.transform.parent = null;
        exploder.ExplodeCracked(this.gameObject);
        tutorialRoomController.currentTargetNumber -= 1;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "PlayerProjectile" || collision.collider.tag == "PlayerBeam")
        {
            //Debug.Log("OnCollisionEnter for: " + collision.collider.name);
            DestroyTarget();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        //Debug.Log("OnTriggerEnter for: " + this.gameObject.name + " by: " + collider.gameObject.name);
        if (collider.tag == "PlayerProjectile" || collider.tag == "PlayerBeam")
        {
            DestroyTarget();
        }
    }
}
