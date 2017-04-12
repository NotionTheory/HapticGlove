using System;
using UnityEngine;

public class DinoClaw : MonoBehaviour
{
    Animation[] anims;
    AnimationState[] states;
    SteamVR_Controller.Device controller;

    private void Start()
    {
        var trackedCtrl = this.GetComponent<SteamVR_TrackedController>();
        this.controller = SteamVR_Controller.Input((int)trackedCtrl.controllerIndex);
        this.anims = GetComponentsInChildren<Animation>();
        this.states = new AnimationState[this.anims.Length];
        for(int i = 0; i < this.anims.Length; ++i)
        {
            this.anims[i].Stop();
            this.states[i] = this.anims[i][this.anims[i].name + "Curl"];
        }
    }

    private void Update()
    {
        var trig = this.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        for(int i = 0; i < this.anims.Length; ++i)
        {
            this.states[i].normalizedTime = trig.x;
            this.states[i].speed = 0;
            this.anims[i].Play();
        }
    }
}
