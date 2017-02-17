using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialRoomController : MonoBehaviour
{
    public GameObject[] targetHolders;
    private int totalTargetNumber = 3;
    public int currentTargetNumber;
    public GameObject targetPrefab;

    public int currentSequenceNumber = 0;
    private float betweenSequenceInterval = 3.0f;
    private float betweenSequenceTimer = 3.0f;
    public AudioSource roundResetAudioSource;
    private bool playedSoundForRound = false;

    private bool endTutorialSequenceStarted = false;
    public AudioSource willBeGradedAudioSource;
    private bool playedWillBeGraded = false;

    private float endOfTutorialTimer = 10.0f;

    public SteamVR_LoadLevel steamVRLoadLevel;

	void Start ()
    {
        ResetTargets();
        betweenSequenceTimer = betweenSequenceInterval;
	}
	
	void Update ()
    {
        if (currentTargetNumber > 0)
        {

        }
        else
        {
            StartNextSequence();
        }
	}

    void StartNextSequence()
    {
        if(betweenSequenceTimer > 0)
        {
            betweenSequenceTimer -= Time.deltaTime;
            if (roundResetAudioSource.isPlaying == false && (betweenSequenceTimer <= roundResetAudioSource.clip.length) && (currentSequenceNumber <= 3) && !playedSoundForRound)
            {
                roundResetAudioSource.Play();
                playedSoundForRound = true;
            }
        } else
        {
            if (currentSequenceNumber <= 3)
            {
                ResetTargets();
                betweenSequenceTimer = betweenSequenceInterval;
                playedSoundForRound = false;
            } else
            {
                EndTutorialSequence();
            }
        }
    }

    void EndTutorialSequence()
    {
        //SteamVR_Fade.Start(Color.black, 5.0f);
        SteamVR_Fade.View(Color.black, 1.0f);

        if (!playedWillBeGraded)
        {
            willBeGradedAudioSource.Play();
            playedWillBeGraded = true;
        }

        if(endOfTutorialTimer > 0)
        {
            endOfTutorialTimer -= Time.deltaTime;
        } else
        {
            // Load Start of Experience
            SteamVR_Fade.View(Color.clear, 0.0f);
            SceneManager.LoadScene("Alleyway");
            //SteamVR_LoadLevel.Begin("Alleyway", false, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f);
            //steamVRLoadLevel.Trigger();
        }
    }

    void ResetTargets()
    {
        for(int i = 0; i < targetHolders.Length; i++)
        {
            GameObject newTarget = (GameObject)Instantiate(targetPrefab, targetHolders[i].transform.position, targetHolders[i].transform.rotation);
            newTarget.transform.parent = targetHolders[i].transform;

            TargetController newTargetController = newTarget.GetComponent<TargetController>();
            newTargetController.tutorialRoomController = this.GetComponent<TutorialRoomController>();

            targetHolders[i].GetComponent<TargetHolderController>().StartingPosition();

            currentTargetNumber++;
        }
        currentSequenceNumber++;
        Debug.Log("Current Sequence Number: " + currentSequenceNumber);
    }
}
