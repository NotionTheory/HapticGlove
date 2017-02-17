using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePatternGameController : MonoBehaviour
{
    [HideInInspector]
    public IState currentState;
    [HideInInspector]
    public PlayState playState;
    [HideInInspector]
    public TransitionState transitionState;

    private void Awake()
    {
        transitionState = new TransitionState(this);
        playState = new PlayState(this);
    }

    void Start()
    {
        currentState = transitionState;
    }

    void Update()
    {
        currentState.UpdateState();
    }
}
