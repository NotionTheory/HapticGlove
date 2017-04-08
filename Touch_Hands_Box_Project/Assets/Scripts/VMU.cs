using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VMU : MonoBehaviour {

    [Range(0, 5)]
    public float BarScale = 4;
    Transform[] bars = new Transform[BeatDetector.BandCount];
    Vector3 scale = new Vector3(0.1f, 0, 0.1f);

	// Use this for initialization
	void Start ()
    {
        for(int i = 0; i < bars.Length; ++i)
        {
            bars[i] = this.transform.GetChild(i);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
    
    public void SetVMU(float[] spectrum)
    {
        for(int i = 0; i < spectrum.Length; ++i)
        {
            scale.y = spectrum[i] * BarScale;
            bars[i].localScale = scale;
        }
    }
}
