using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState {

    void UpdateState();

    void ToTransitionState();

    void ToPlayState();
}
