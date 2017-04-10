using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NixieTube))]
[RequireComponent(typeof(BeatDetector))]
public class BeatFollower : MonoBehaviour
{
    public BeatDetector beater;
    public NixieTube digit;
    [Range(60, 480)]
    public float TargetBPM = 120;
    public bool DoubleBeat = false;
    [Range(0, 0.5f)]
    public float PhaseShift = 0;
    float lastPhaseShift;

    IndicatorLamp[] lamps;
    int taps = 0;
    int tappedMeasures = 0;
    int beats = 0;
    int beatedMeasures = 0;
    float lastBeat;
    // Use this for initialization
    void Start()
    {
        lamps = GetComponentsInChildren<IndicatorLamp>();
    }

    // Update is called once per frame
    void Update()
    {
        lastBeat += PhaseShift - lastPhaseShift;
        if(beater.CurrentTime >= nextBeat)
        {
            beats = (beats + 1) % lamps.Length;
            if(beats == 0)
            {
                ++beatedMeasures;
            }
            digit.AdvanceTo(4);
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
        taps = (taps + 1) % lamps.Length;
        if(taps == 0)
        {
            ++tappedMeasures;
        }
        for(int i = 0; i < lamps.Length; ++i)
        {
            lamps[i].IsOn = taps >= i;
        }
    }
}
