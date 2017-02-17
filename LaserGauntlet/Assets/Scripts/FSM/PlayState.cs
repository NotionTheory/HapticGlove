using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : IState
{
    private readonly StatePatternGameController gameController;
    private int transitionNumber;

    public PlayState(StatePatternGameController controller)
    {
        gameController = controller;
    }

    public void UpdateState()
    {

    }

    public void ToTransitionState()
    {
        Debug.Log("Can't transition to same state");
    }

    public void ToPlayState()
    {
        gameController.currentState = gameController.playState;
    }
}