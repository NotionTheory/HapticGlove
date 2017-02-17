using UnityEngine;
using System.Collections;

public class TargetHolderController : MonoBehaviour
{
    public int speed = 0;
    Animator animationController;

    private Vector3 startingPosition;

	void Start ()
    {
        startingPosition = this.transform.position;
        animationController = this.GetComponent<Animator>();
        animationController.SetInteger("Speed", speed);
    }
	
	void Update ()
    {
	
	}

    public void StartingPosition()
    {
        this.transform.position = startingPosition;
    }
}
