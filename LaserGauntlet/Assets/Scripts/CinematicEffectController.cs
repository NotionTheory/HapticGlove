using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SWS;

public class CinematicEffectController : MonoBehaviour
{
    public AudioSource MusicAudioSource;
    public AudioSource DialogueAudioSource;
    public AudioSource SFXAudioSource;

    public GameObject CameraRig;
    public Camera mainCamera;
    public GameObject blackQuad;
    public GameObject transitionGameObjects;
    public GameObject transitionAnimationGOs;

    public GauntletController gauntletController;

    public TransitionToSceneOnTrigger transitionToSceneOnTrigger;

    public GameObject reflectionProbeGroup;
    public ReflectionProbe[] reflectionProbes;

    public int currentSequenceNumber = 0;

    private bool switchToBlack = false;
    private bool isScreenBlack = false;

    bool firstSequenceStarted = false;
    bool firstSequenceFinished = false;
    bool firstMusicStarted = false;
    bool firstDialogueStarted = false;
    bool firstSFXStarted = false;

    public GameObject[] firstPlaySequenceEnemies;

    bool secondSequenceStarted = false;
    bool secondSequenceFinished = false;
    bool secondMusicStarted = false;
    bool secondDialogueStarted = false;
    bool secondSFXStarted = false;
    public AudioClip secondDialogueClip;

    public GameObject[] secondPlaySequenceEnemies;

    bool thirdSequenceStarted = false;
    bool thirdSequenceFinished = false;
    bool thirdMusicStarted = false;
    bool thirdDialogueStarted = false;
    bool thirdSFXStarted = false;
    public AudioClip thirdDialogueClip;
    public AudioClip thirdMusicClip;
    public GameObject secondCameraRigPosition;

    public GameObject jetEnemy;

    bool lastAlleySequenceStarted = false;
    bool lastAlleySequenceFinished = false;
    bool lastAlleyMusicStarted = false;
    bool lastAlleyDialogueStarted = false;
    bool lastAlleySFXStarted = false;
    public AudioClip lastAlleyDialogueClip;
    public Material secondSkybox;

    public GameObject Alleyway;
    public GameObject City;

    public Material thirdSkybox;
    public AudioClip fourthMusicClip;
    public GameObject thirdCameraRigPosition;
    public ParticleSystem leftAnimatedArrow;
    public ParticleSystem rightAnimatedArrow;

    public GameObject[] firstCityPlaySequenceEnemies;

    bool firstCitySequenceStarted = false;
    bool firstCitySequenceFinished = false;
    bool firstCityMusicStarted = false;
    bool firstCityDialogueStarted = false;
    bool firstCitySFXStarted = false;

    public GameObject spiderSwarm01;
    public AudioSource lookAtSwarmAudioSource;

    public GameObject[] thirdCityPlaySequenceEnemies;

    bool thirdCitySequenceStarted = false;
    bool thirdCitySequenceFinished = false;
    bool thirdCityMusicStarted = false;
    bool thirdCityDialogueStarted = false;
    bool thirdCitySFXStarted = false;
    public AudioClip windAudioClip;

    bool lastCitySequenceStarted = false;
    bool lastCitySequenceFinished = false;
    bool lastCityMusicStarted = false;
    bool lastCityDialogueStarted = false;
    bool lastCitySFXStarted = false;
    public AudioClip lastCityDialogueClip;
    //public AudioClip lastAudioClip;
    public GameObject notionTheoryLogo;
    public ScoreController scoreController;

    private int defaultCullingMask;

    void Start()
    {
        ReflectionProbeUpdate();
        defaultCullingMask = mainCamera.cullingMask;
        //Debug.Log("defaultCullingMask: " + defaultCullingMask);
        // -2049 for all except transition
        // 2048 for transition

        switchToBlack = true;
        isScreenBlack = true;
    }

    void Update()
    {

        // DETERMINE IF SCREEN SHOULD BE BLACK
        if (switchToBlack == true && isScreenBlack == false)
        {
            MakeCameraBlack();
            isScreenBlack = true;
        }
        else if (switchToBlack == false && isScreenBlack == true)
        {
            MakeCameraClear();
            isScreenBlack = false;
        }

        // FIRST BLACK SEQUENCE
        if (currentSequenceNumber == 0 && firstSequenceFinished == false)
        {
            StartCoroutine(FirstSequence());
        }

        if(currentSequenceNumber == 1 & firstSequenceFinished == true)
        {
            switchToBlack = false;
        }

        // FIRST PLAY SEQUENCE
        if (currentSequenceNumber == 1)
        {
            int explodedCount = 0;
            for (int i = 0; i < firstPlaySequenceEnemies.Length; i++)
            {
                if (firstPlaySequenceEnemies[i].GetComponent<DroneController>().exploded == true)
                {
                    explodedCount++;
                } else if (explodedCount > 0)
                {
                    explodedCount--;
                }
            }
            if (explodedCount >= firstPlaySequenceEnemies.Length)
            {
                currentSequenceNumber = 2;
            }
        }

        // SECOND BLACK SEQUENCE
        if (currentSequenceNumber == 2 && secondSequenceFinished == false)
        {
            //Debug.Log("Switch to black: " + switchToBlack);
            StartCoroutine(SecondSequence());
        }
        if(currentSequenceNumber == 2 && secondSequenceFinished == true)
        {
            switchToBlack = false;
            //blackQuad.SetActive(false);
        }

        // SECOND PLAY SEQUENCE
        if (currentSequenceNumber == 2)
        {
            int explodedCount = 0;
            for (int i = 0; i < secondPlaySequenceEnemies.Length; i++)
            {
                if (secondPlaySequenceEnemies[i].GetComponent<DroneController>().exploded == true)
                {
                    explodedCount++;
                }
                else if (explodedCount > 0)
                {
                    explodedCount--;
                }
            }
            if (explodedCount >= secondPlaySequenceEnemies.Length)
            {
                currentSequenceNumber = 3;
            }
        }

        // THIRD BLACK SEQUENCE
        if (currentSequenceNumber == 3 && thirdSequenceFinished == false)
        {
            StartCoroutine(ThirdSequence());
        }
        if(currentSequenceNumber == 3 && thirdSequenceFinished == true)
        {
            switchToBlack = false;
            //blackQuad.SetActive(false);
        }

        // FINAL ALLEY SEQUENCE
        if(currentSequenceNumber == 4 && lastAlleySequenceFinished == false)
        {
            StartCoroutine(LastAlleySequence());
        }

        // FIRST CITY PLAY SEQUENCE
        if(currentSequenceNumber == 5)
        {
            int explodedCount = 0;
            for (int i = 0; i < firstCityPlaySequenceEnemies.Length; i++)
            {
                if (firstCityPlaySequenceEnemies[i].GetComponent<DroneController>().exploded == true)
                {
                    explodedCount++;
                }
                else if (explodedCount > 0)
                {
                    explodedCount--;
                }
            }
            if (explodedCount >= firstCityPlaySequenceEnemies.Length)
            {
                currentSequenceNumber = 6;
            }
        }

        // SECOND CITY (SWARM) PLAY SEQUENCE
        if(currentSequenceNumber == 6 && firstCitySequenceFinished == false)
        {
            StartCoroutine(FirstCitySequence());
        }
        if(currentSequenceNumber == 6 && firstCitySequenceFinished == true)
        {
            switchToBlack = false;
        }

        // THIRD SEQUENCE
        if (currentSequenceNumber == 7 && thirdCitySequenceFinished == false)
        {
            StartCoroutine(ThirdCitySequence());
        }

        if (currentSequenceNumber == 7 && thirdCitySequenceFinished == true)
        {
            switchToBlack = false;
        }

        // THIRD CITY PLAY SEQUENCE
        if (currentSequenceNumber == 7)
        {
            //blackQuad.SetActive(false);
            int explodedCount = 0;
            for (int i = 0; i < thirdCityPlaySequenceEnemies.Length; i++)
            {
                if (thirdCityPlaySequenceEnemies[i].GetComponent<DroneController>().exploded == true)
                {
                    explodedCount++;
                }
                else if (explodedCount > 0)
                {
                    explodedCount--;
                }
            }
            if (explodedCount >= thirdCityPlaySequenceEnemies.Length)
            {
                currentSequenceNumber = 8;
            }
        }

        if(currentSequenceNumber == 8 && lastCitySequenceFinished == false)
        {
            StartCoroutine(LastCitySequence());
        }
    }


    void ReflectionProbeUpdate()
    {
        for (int i = 0; i < reflectionProbes.Length; i++)
        {
            reflectionProbes[i].RenderProbe();
        }
    }

    void MakeCameraBlack()
    {
        //blackQuad.SetActive(true);
        transitionGameObjects.SetActive(true);
        //mainCamera.cullingMask = 2048;
        mainCamera.cullingMask = 2560;
    }

    void MakeCameraClear()
    {
        //blackQuad.SetActive(false);
        transitionGameObjects.SetActive(false);
        mainCamera.cullingMask = -2049;
    }

    IEnumerator FirstSequence()
    {
        switchToBlack = true;
        //blackQuad.SetActive(true);

        firstSequenceStarted = true;
        gauntletController.TurnOffScript();

        if (firstMusicStarted == false)
        {
            firstMusicStarted = true;
            MusicAudioSource.Play();
        }

        yield return new WaitForSeconds(3.8f);

        if (firstDialogueStarted == false)
        {
            firstDialogueStarted = true;
            DialogueAudioSource.Play();
        }

        yield return new WaitForSeconds(3.8f);
        //yield return new WaitForSeconds(5.0f);

        if (firstSFXStarted == false)
        {
            firstSFXStarted = true;
            SFXAudioSource.Play();
        }

        yield return new WaitForSeconds(0.75f);

        //switchToBlack = false;
        //blackQuad.SetActive(false);

        gauntletController.enabled = true;

        for (int i = 0; i < firstPlaySequenceEnemies.Length; i++)
        {
            firstPlaySequenceEnemies[i].SetActive(true);
        }
        firstSequenceFinished = true;
        currentSequenceNumber = 1;
    }

    IEnumerator SecondSequence()
    {
        //Debug.Log("SecondSequence started");

        yield return new WaitForSeconds(0.5f);

        secondSequenceStarted = true;

        switchToBlack = true;
        //blackQuad.SetActive(true);
        gauntletController.TurnOffScript();

        yield return new WaitForSeconds(0.5f);

        if (secondDialogueStarted == false)
        {
            secondDialogueStarted = true;
            DialogueAudioSource.clip = secondDialogueClip;
            DialogueAudioSource.Play();
        }

        yield return new WaitForSeconds(4.94f);
        //yield return new WaitForSeconds(3.1f);
        //yield return new WaitForSeconds(2.75f);

        if (secondSFXStarted == false)
        {
            secondSFXStarted = true;
            SFXAudioSource.Play();
        }

        yield return new WaitForSeconds(0.55f);

        //switchToBlack = false;
        gauntletController.enabled = true;

        for (int i = 0; i < secondPlaySequenceEnemies.Length; i++)
        {
            secondPlaySequenceEnemies[i].SetActive(true);
        }
        secondSequenceFinished = true;
    }

    IEnumerator ThirdSequence()
    {
        //Debug.Log("ThirdSequence started");
        yield return new WaitForSeconds(0.5f);

        switchToBlack = true;
        //blackQuad.SetActive(true);

        thirdSequenceStarted = true;
        gauntletController.TurnOffScript();

        yield return new WaitForSeconds(0.5f);

        if (thirdDialogueStarted == false)
        {
            thirdDialogueStarted = true;
            DialogueAudioSource.clip = thirdDialogueClip;
            DialogueAudioSource.Play();
        }

        yield return new WaitForSeconds(3.8f);

        if (thirdMusicStarted == false)
        {
            thirdMusicStarted = true;
            MusicAudioSource.clip = thirdMusicClip;
            MusicAudioSource.Play();
        }

        //CameraRig.transform.position = secondCameraRigPosition.transform.position;
        //CameraRig.transform.rotation = secondCameraRigPosition.transform.rotation;

        switchToBlack = false;

        gauntletController.enabled = true;

        jetEnemy.SetActive(true);
        //RenderSettings.skybox = secondSkybox;
        //ReflectionProbeUpdate();

        thirdSequenceFinished = true;
    }

    IEnumerator LastAlleySequence()
    {
        switchToBlack = true;
        //blackQuad.SetActive(true);

        thirdSequenceStarted = true;
        gauntletController.TurnOffScript();

        yield return new WaitForSeconds(0.5f);

        if (lastAlleyDialogueStarted == false)
        {
            MusicAudioSource.Stop();
            lastAlleyDialogueStarted = true;
            DialogueAudioSource.clip = lastAlleyDialogueClip;
            DialogueAudioSource.Play();
        }

        yield return new WaitForSeconds(3.0f);
        //yield return new WaitForSeconds(9.75f);

        switchToBlack = false;

        //SceneManager.LoadScene("City");
        Alleyway.SetActive(false);
        City.SetActive(true);
        RenderSettings.skybox = thirdSkybox;
        CameraRig.transform.position = thirdCameraRigPosition.transform.position;
        CameraRig.transform.rotation = thirdCameraRigPosition.transform.rotation;

        //rightAnimatedArrow.main.startRotationY = new ParticleSystem.MinMaxCurve(90.0f);
        //leftAnimatedArrow.main.startRotationY = new ParticleSystem.MinMaxCurve(-90.0f);

        var leftMain = leftAnimatedArrow.main;
        //leftMain.startRotationYMultiplier = -90.0f;
        leftMain.startRotationY = new ParticleSystem.MinMaxCurve(-180.0f);

        var rightMain = rightAnimatedArrow.main;
        //rightMain.startRotationYMultiplier = 90.0f;
        rightMain.startRotationY = new ParticleSystem.MinMaxCurve(180.0f);

        reflectionProbeGroup.transform.position = new Vector3(10.0f, reflectionProbeGroup.transform.position.y, reflectionProbeGroup.transform.position.z);
        ReflectionProbeUpdate();
        if (lastAlleyMusicStarted == false)
        {
            lastAlleyMusicStarted = true;
            MusicAudioSource.clip = fourthMusicClip;
            MusicAudioSource.Play();
        }

        for (int i = 0; i < firstCityPlaySequenceEnemies.Length; i++)
        {
            firstCityPlaySequenceEnemies[i].SetActive(true);
        }

        lastAlleySequenceFinished = true;
        gauntletController.enabled = true;
        currentSequenceNumber = 5;
    }

    IEnumerator FirstCitySequence()
    {
        yield return new WaitForSeconds(0.5f);

        switchToBlack = true;
        //blackQuad.SetActive(true);

        firstCitySequenceStarted = true;
        gauntletController.TurnOffScript();

        yield return new WaitForSeconds(0.5f);

        if (firstCitySFXStarted == false)
        {
            firstCitySFXStarted = true;
            lookAtSwarmAudioSource.Play();
        }

        yield return new WaitForSeconds(2.0f);

        //switchToBlack = false;
        //blackQuad.SetActive(false);

        gauntletController.enabled = true;

        spiderSwarm01.SetActive(true);

        firstCitySequenceFinished = true;
    }

    IEnumerator ThirdCitySequence()
    {
        yield return new WaitForSeconds(0.5f);

        switchToBlack = true;
        //blackQuad.SetActive(true);

        thirdCitySequenceStarted = true;
        gauntletController.TurnOffScript();

        if(thirdCityMusicStarted == false)
        {
            MusicAudioSource.Stop();
            thirdCityMusicStarted = true;
            MusicAudioSource.clip = windAudioClip;
            MusicAudioSource.Play();
        }

        yield return new WaitForSeconds(1.75f);

        if (thirdCitySFXStarted == false)
        {
            thirdCitySFXStarted = true;
            SFXAudioSource.Play();
        }

        yield return new WaitForSeconds(0.75f);

        //switchToBlack = false;
        //blackQuad.SetActive(false);

        gauntletController.enabled = true;

        for (int i = 0; i < thirdCityPlaySequenceEnemies.Length; i++)
        {
            thirdCityPlaySequenceEnemies[i].SetActive(true);
        }

        thirdCitySequenceFinished = true;
    }

    IEnumerator LastCitySequence()
    {
        yield return new WaitForSeconds(0.5f);

        switchToBlack = true;
        transitionAnimationGOs.SetActive(false);

        if (lastCityDialogueStarted == false)
        {
            MusicAudioSource.Stop();
            lastCityDialogueStarted = true;
            //DialogueAudioSource.clip = lastCityDialogueClip;
            //DialogueAudioSource.Play();
        }
        if(lastCitySFXStarted == false)
        {
            lastCitySFXStarted = true;
            SFXAudioSource.Play();
        }

        yield return new WaitForSeconds(0.75f);

        notionTheoryLogo.SetActive(true);

        if (lastCityMusicStarted == false)
        {
            lastCityMusicStarted = true;
            MusicAudioSource.clip = fourthMusicClip;
            MusicAudioSource.loop = false;
            MusicAudioSource.Play();
        }

        lastCitySequenceFinished = true;
        scoreController.DetermineLetterGrade();
    }
}
