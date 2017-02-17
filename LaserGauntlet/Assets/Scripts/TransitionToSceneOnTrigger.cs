using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToSceneOnTrigger : MonoBehaviour
{
    public string level;
    AsyncOperation async;

    public CinematicEffectController cinematicEffectController;

	void Start ()
    {
        //async = new AsyncOperation();
        //StartLoading();
	}
	
	void Update ()
    {
        //StartLoading();
	}

    public void StartLoading()
    {
        StartCoroutine("load");
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Boss")
        {
            //SceneManager.LoadScene(level);
            cinematicEffectController.currentSequenceNumber = 4;
            cinematicEffectController.jetEnemy.SetActive(false);
        }
    }

    void LoadAsync()
    {
        SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);
    }

    IEnumerable load()
    {
        //SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);
        async = SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);
        async.allowSceneActivation = false;
        yield return async;
    }

    public void ActiveScene()
    {
        async.allowSceneActivation = true;
    }
}
