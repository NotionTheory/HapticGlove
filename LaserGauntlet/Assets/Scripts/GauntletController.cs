using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using VRTK;

public class GauntletController : MonoBehaviour
{
    public VRTK_ControllerActions controllerActions;
    public GloveComponent gloveComponent;

    public GameObject projectileSpawn;

    public GameObject pulsePrefab;
    public AudioClip pulseAudio;
    public GameObject chargePrefab;
    public GameObject laserBeam;

    public bool charging = false;
    private bool firing = false;
    private float beamChargeMax = 1.5f;
    private float beamChargeLevel = 0.0f;
    private float beamTimeRemaining = 0.0f;
    public AudioSource chargeAudioSource;

    private float pulseCooldownInterval = 1.0f;
    private float pulseCooldownTimer = 0.0f;
    private bool canShootPulse = true;

    private bool debugMode = false;

    void Start()
    {
        if(GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("VRTK_ControllerEvents_ListenerExample is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }

        //Setup controller event listeners
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);

        GetComponent<VRTK_ControllerEvents>().TriggerTouchStart += new ControllerInteractionEventHandler(DoTriggerTouchStart);
        GetComponent<VRTK_ControllerEvents>().TriggerTouchEnd += new ControllerInteractionEventHandler(DoTriggerTouchEnd);

        GetComponent<VRTK_ControllerEvents>().TriggerHairlineStart += new ControllerInteractionEventHandler(DoTriggerHairlineStart);
        GetComponent<VRTK_ControllerEvents>().TriggerHairlineEnd += new ControllerInteractionEventHandler(DoTriggerHairlineEnd);

        GetComponent<VRTK_ControllerEvents>().TriggerClicked += new ControllerInteractionEventHandler(DoTriggerClicked);
        GetComponent<VRTK_ControllerEvents>().TriggerUnclicked += new ControllerInteractionEventHandler(DoTriggerUnclicked);

        GetComponent<VRTK_ControllerEvents>().TriggerAxisChanged += new ControllerInteractionEventHandler(DoTriggerAxisChanged);

        //GetComponent<VRTK_ControllerEvents>().ApplicationMenuPressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
        //GetComponent<VRTK_ControllerEvents>().ApplicationMenuReleased += new ControllerInteractionEventHandler(DoApplicationMenuReleased);
        GetComponent<VRTK_ControllerEvents>().ButtonOnePressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
        GetComponent<VRTK_ControllerEvents>().ButtonOneReleased += new ControllerInteractionEventHandler(DoApplicationMenuReleased);

        GetComponent<VRTK_ControllerEvents>().GripPressed += new ControllerInteractionEventHandler(DoGripPressed);
        GetComponent<VRTK_ControllerEvents>().GripReleased += new ControllerInteractionEventHandler(DoGripReleased);

        GetComponent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(DoTouchpadPressed);
        GetComponent<VRTK_ControllerEvents>().TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadReleased);

        GetComponent<VRTK_ControllerEvents>().TouchpadTouchStart += new ControllerInteractionEventHandler(DoTouchpadTouchStart);
        GetComponent<VRTK_ControllerEvents>().TouchpadTouchEnd += new ControllerInteractionEventHandler(DoTouchpadTouchEnd);

        GetComponent<VRTK_ControllerEvents>().TouchpadAxisChanged += new ControllerInteractionEventHandler(DoTouchpadAxisChanged);

        GetComponent<VRTK_ControllerEvents>().ControllerEnabled += new ControllerInteractionEventHandler(DoControllerEnabled);
        GetComponent<VRTK_ControllerEvents>().ControllerDisabled += new ControllerInteractionEventHandler(DoControllerDisabled);
    }

    void Update()
    {
        //Debug.Log("Charing: " + charging + " Beam Charge Level: " + beamChargeLevel + " Beam Time Remaining: " + beamTimeRemaining);

        // DEMO OPERATOR CONTROLS
        DemoOperatorControls();

        //// BEAM ////
        // Charging
        if(charging == true && firing == false)
        {
            if(beamChargeLevel <= 0.0f)
            {
                chargeAudioSource.Play();
            }
            ChargeBeam();
        }
        else
        // Stop firing and start charging
        if(charging == true && firing == true)
        {
            beamChargeLevel = 0.0f;
            beamTimeRemaining = 0.0f;
            firing = false;
            laserBeam.SetActive(false);
        }
        else
        // Minimum charge was met
        if(charging == false && beamChargeLevel >= 0.5f)
        {
            FireBeam();
            chargeAudioSource.Stop();
        }
        else
        // Minimum charge was not met
        if(charging == false && firing == false && beamChargeLevel < 0.5f)
        {
            DrainBeam();
            chargeAudioSource.Stop();
        }

        HapticsForViveController();

        //// PULSE ////
        if(pulseCooldownTimer > 0.0f)
        {
            pulseCooldownTimer -= Time.deltaTime;
        }
        if(pulseCooldownTimer <= 0.0f)
        {
            canShootPulse = true;
        }
    }

    void DemoOperatorControls()
    {
        if(Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Reset To Tutorial");
            SceneManager.LoadScene("Tutorial");
        }

        if(Input.GetKey(KeyCode.Equals) && Input.GetKey(KeyCode.Space))
        {
            debugMode = true;
            Debug.Log("Debug Mode is now " + debugMode + ". Right Controller Trigger Activated.");
        }
        if(Input.GetKey(KeyCode.Minus) && Input.GetKey(KeyCode.Space))
        {
            debugMode = false;
            Debug.Log("Debug Mode is now " + debugMode + ". Right Controller Trigger Activated.");
        }
    }

    public void TurnOffScript()
    {
        beamChargeLevel = 0.0f;
        beamTimeRemaining = 0.0f;
        charging = false;
        firing = false;
        laserBeam.SetActive(false);
        this.enabled = false;
    }


    private const float 
        START_CHARGE = 0.5f, 
        CHARGE_DIFF = 0.25f;

    public void HapticsForViveController()
    {
        ///// Haptics

        //// ALL ON WHEN CAN MINIMUM FIRE
        //if (beamChargeLevel >= 0.5f)
        //{
        //    //controllerActions.TriggerHapticPulse((beamChargeLevel * beamChargeLevel * beamChargeLevel) / (beamChargeMax * beamChargeMax * beamChargeMax));
        //    for(int i = 0; i < gloveComponent.motors.Length; i++)
        //    {
        //        gloveComponent.motors[i] = true;
        //    }
        //} else
        //{
        //    for (int i = 0; i < gloveComponent.motors.Length; i++)
        //    {
        //        gloveComponent.motors[i] = false;
        //    }
        //}

        // EACH FINGER GOES AS CHARGE GOES UP
        for(int i = 0; i < gloveComponent.motors.Length; ++i)
        {
            gloveComponent.motors[i] = (beamChargeLevel >= START_CHARGE + i * CHARGE_DIFF);
        }
    }

    public void FirePulse()
    {
        GameObject pulseProjectile = (GameObject)Instantiate(pulsePrefab, projectileSpawn.transform.position, Quaternion.Euler(projectileSpawn.transform.forward));
        AudioSource.PlayClipAtPoint(pulseAudio, projectileSpawn.transform.position);
        pulseProjectile.GetComponent<Rigidbody>().velocity = 50.0f * projectileSpawn.transform.forward;
        pulseCooldownTimer = pulseCooldownInterval;
        canShootPulse = false;
    }

    public void ChargeBeam()
    {
        //Debug.Log("Inside ChargingBeam");

        if(beamChargeLevel <= beamChargeMax && firing == false)
        {
            beamChargeLevel += Time.deltaTime;
            beamTimeRemaining = beamChargeLevel;
        }
    }

    public void DrainBeam()
    {
        //Debug.Log("Inside DrainBeam");

        beamChargeLevel = 0.0f;
        laserBeam.SetActive(false);
    }

    public void FireBeam()
    {
        //Debug.Log("Inside FireBeam");

        if(charging == true)
        {
            beamTimeRemaining = 0.0f;
            firing = false;
        }

        if(beamTimeRemaining > 0.0f)
        {
            laserBeam.SetActive(true);
            beamTimeRemaining -= Time.deltaTime;
            firing = true;
            //beamChargeLevel = 0.0f;
        }
        else
        {
            firing = false;
            beamChargeLevel = 0.0f;
            laserBeam.SetActive(false);
        }
    }

    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        //Debug.Log("Controller on index '" + index + "' " + button + " has been " + action + " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
    }

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "pressed", e);
        if(debugMode == true)
        {
            charging = true;
        }
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "released", e);
        if(debugMode == true)
        {
            charging = false;
        }
    }

    private void DoTriggerTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "touched", e);
    }

    private void DoTriggerTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "untouched", e);
    }

    private void DoTriggerHairlineStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "hairline start", e);
    }

    private void DoTriggerHairlineEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "hairline end", e);
    }

    private void DoTriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "clicked", e);
    }

    private void DoTriggerUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "unclicked", e);
    }

    private void DoTriggerAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TRIGGER", "axis changed", e);
    }

    private void DoApplicationMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "APPLICATION MENU", "pressed down", e);
    }

    private void DoApplicationMenuReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "APPLICATION MENU", "released", e);
    }

    private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "pressed down", e);
    }

    private void DoGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "GRIP", "released", e);
    }

    private void DoTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "pressed down", e);
        if(canShootPulse == true && charging == false && laserBeam.activeSelf == false && this.isActiveAndEnabled)
        {
            FirePulse();
        }
    }

    private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "released", e);
    }

    private void DoTouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "touched", e);
    }

    private void DoTouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "untouched", e);
    }

    private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "TOUCHPAD", "axis changed", e);
    }

    private void DoControllerEnabled(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "CONTROLLER STATE", "ENABLED", e);
    }

    private void DoControllerDisabled(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(e.controllerIndex, "CONTROLLER STATE", "DISABLED", e);
    }
}
