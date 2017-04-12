using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatFollower : MonoBehaviour
{
    public BeatDetector beater;
    [Range(60, 480)]
    public float TargetBPM = 120;
    public bool DoubleBeat = false;
    [Range(0, 0.5f)]
    public float PhaseShift = 0;

    public UnityEngine.Events.UnityEvent OnBeat;
    float lastPhaseShift;
    
    float lastBeat;

    void Start()
    {
    }
    
    void Update()
    {
        lastBeat += PhaseShift - lastPhaseShift;
        if(beater.CurrentTime >= nextBeat)
        {
            if(OnBeat != null)
            {
                OnBeat.Invoke();
            }
            lastBeat = beater.CurrentTime;
        }
        lastPhaseShift = PhaseShift;
    }

    float nextBeat
    {
        get
        {
            return lastBeat + dBeat;
        }
    }

    float dBeat
    {
        get
        {
            return (DoubleBeat ? 30 : 60) / TargetBPM;
        }
    }

    public void Tap()
    {
    }
}
