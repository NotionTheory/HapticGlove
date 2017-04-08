using System;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class OnSpectrumEventHandler : UnityEngine.Events.UnityEvent<float[]>
{

}

[RequireComponent(typeof(AudioSource))]
public class BeatDetector : MonoBehaviour
{
    public AudioSource Source;
    [Range(6, 13)]
    public int BufferMagnitude = 10;
    float Sensitivity = 0.1f;

    public const int BandCount = 12;


    int blipDelayLen = 16;
    int[] blipDelay;

    int updatesSinceLastBeat = 0;
    float framePeriod;

    /* storage space */
    int colmax = 120;
    float[] spectrum;
    float[] averages;
    float[] acVals;
    float[] onsets;
    float[] scorefun;
    float[] dobeat;
    int now = 0;
    // time index for circular buffer within above

    float[] spec;
    // the spectrum of the previous step

    /* Autocorrelation structure */
    int maxlag = 100;
    // (in frames) largest lag to track
    float decay = 0.997f;
    // smoothing constant for running average

    private float[] delays;
    private float[] outputs;
    private int indx;

    private float[] bpms;
    private float[] rweight;
    private float wmidbpm = 120f;

    public void newVal(float val)
    {

        delays[indx] = val;

        // update running autocorrelator values
        for(int i = 0; i < maxlag; ++i)
        {
            int delix = (indx - i + maxlag) % maxlag;
            outputs[i] += (1 - decay) * (delays[indx] * delays[delix] - outputs[i]);
        }

        if(++indx == maxlag)
            indx = 0;
    }

    int BufferSize
    {
        get
        {
            return (int)Mathf.Pow(2, BufferMagnitude);
        }
    }

    // read back the current autocorrelator value at a particular lag
    public float autoco(int del)
    {
        float blah = rweight[del] * outputs[del];
        return blah;
    }

    private float alph;
    // trade-off constant between tempo deviation penalty and onset strength

    [Header ("Events")]
    public UnityEngine.Events.UnityEvent OnBeat;
    public OnSpectrumEventHandler OnSpectrum;

    void Start()
    {
        blipDelay = new int[blipDelayLen];
        onsets = new float[colmax];
        scorefun = new float[colmax];
        dobeat = new float[colmax];
        spectrum = new float[BufferSize];
        averages = new float[BandCount];
        acVals = new float[maxlag];
        alph = 100 * Sensitivity;

        Source = GetComponent<AudioSource>();

        framePeriod = (float)BufferSize / samplingRate;

        //initialize record of previous spectrum
        spec = new float[BandCount];
        for(int i = 0; i < BandCount; ++i)
        {
            spec[i] = 100.0f;
        }

        delays = new float[maxlag];
        outputs = new float[maxlag];
        indx = 0;

        // calculate a log-lag gaussian weighting function, to prefer tempi around 120 bpm
        bpms = new float[maxlag];
        rweight = new float[maxlag];
        for(int i = 0; i < maxlag; ++i)
        {
            bpms[i] = 60.0f / (framePeriod * (float)i);
            //Debug.Log(bpms[i]);
            // weighting is Gaussian on log-BPM axis, centered at wmidbpm, SD = woctavewidth octaves
            rweight[i] = Mathf.Exp(-0.5f * Mathf.Pow(Mathf.Log(bpms[i] / wmidbpm) / Mathf.Log(2.0f) / BandWidth, 2.0f));
        }
    }

    long CurrentTimeMillis
    {
        get
        {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds;
        }
    }

    int samplingRate
    {
        get
        {
            return Source.clip.frequency;
        }
    }

    void Update()
    {
        if(Source.isPlaying)
        {
            Source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            ComputeAverages(spectrum);
            if(OnSpectrum != null)
            {
                OnSpectrum.Invoke(averages);
            }

            /* calculate the value of the onset function in this frame */
            float onset = 0;
            for(int i = 0; i < BandCount; i++)
            {
                float bandDecibels = 0.025f * Mathf.Max(-100.0f, 20.0f * Mathf.Log10(averages[i]) + 160); // dB value of this band
                float deltaBandDecibels = bandDecibels - spec[i]; // dB increment since last frame
                spec[i] = bandDecibels; // record this frome to use next time around
                onset += deltaBandDecibels; // onset function is the sum of dB increments
            }

            onsets[now] = onset;

            /* update autocorrelator and find peak lag = current tempo */
            newVal(onset);
            // record largest value in (weighted) autocorrelation as it will be the tempo
            float aMax = 0.0f;
            int tempopd = 0;
            //float[] acVals = new float[maxlag];
            for(int i = 0; i < maxlag; ++i)
            {
                float acVal = Mathf.Sqrt(autoco(i));
                if(acVal > aMax)
                {
                    aMax = acVal;
                    tempopd = i;
                }
                // store in array backwards, so it displays right-to-left, in line with traces
                acVals[maxlag - 1 - i] = acVal;
            }

            /* calculate DP-ish function to update the best-score function */
            float smax = -999999;
            int smaxix = 0;
            // weight can be varied dynamically with the mouse
            alph = 100 * Sensitivity;
            // consider all possible preceding beat times from 0.5 to 2.0 x current tempo period
            for(int i = tempopd / 2; i < Mathf.Min(colmax, 2 * tempopd); ++i)
            {
                // objective function - this beat's cost + score to last beat + transition penalty
                float score = onset + scorefun[(now - i + colmax) % colmax] - alph * Mathf.Pow(Mathf.Log((float)i / tempopd), 2);
                // keep track of the best-scoring predecesor
                if(score > smax)
                {
                    smax = score;
                    smaxix = i;
                }
            }

            scorefun[now] = smax;
            // keep the smallest value in the score fn window as zero, by subtracing the min val
            float smin = scorefun[0];
            for(int i = 0; i < colmax; ++i)
                if(scorefun[i] < smin)
                    smin = scorefun[i];
            for(int i = 0; i < colmax; ++i)
                scorefun[i] -= smin;

            /* find the largest value in the score fn window, to decide if we emit a blip */
            smax = scorefun[0];
            smaxix = 0;
            for(int i = 0; i < colmax; ++i)
            {
                if(scorefun[i] > smax)
                {
                    smax = scorefun[i];
                    smaxix = i;
                }
            }

            // dobeat array records where we actally place beats
            dobeat[now] = 0;  // default is no beat this frame
            ++updatesSinceLastBeat;
            // if current value is largest in the array, probably means we're on a beat
            if(smaxix == now)
            {
                //tapTempo();
                // make sure the most recent beat wasn't too recently
                if(updatesSinceLastBeat > tempopd / 4)
                {
                    OnBeat.Invoke();
                    blipDelay[0] = 1;
                    // record that we did actually mark a beat this frame
                    dobeat[now] = 1;
                    // reset counter of frames since last beat
                    updatesSinceLastBeat = 0;
                }
            }

            /* update column index (for ring buffer) */
            if(++now == colmax)
                now = 0;
        }
    }

    float BandWidth
    {
        get
        {
            return (2f / BufferSize) * (samplingRate / 2f);
        }
    }

    int FrequencyIndex(int freq)
    {
        if(freq < BandWidth / 2)
        {
            return 0;
        }
        else if(freq > samplingRate / 2 - BandWidth / 2)
        {
            return (BufferSize / 2);
        }
        else
        {
            float fraction = (float)freq / (float)samplingRate;
            int i = (int)System.Math.Round(BufferSize * fraction);
            return i;
        }
    }

    public void ComputeAverages(float[] data)
    {
        for(int i = 0; i < BandCount; i++)
        {
            float avg = 0;
            int lowFreq = 0;
            if(i > 0)
            {
                lowFreq = (int)((samplingRate / 2) / Mathf.Pow(2, BandCount - i));
            }
            int hiFreq = (int)((samplingRate / 2) / Mathf.Pow(2, BandCount - i - 1));
            int lowBound = FrequencyIndex(lowFreq);
            int hiBound = FrequencyIndex(hiFreq);
            for(int j = lowBound; j <= hiBound; j++)
            {
                avg += data[j];
            }
            avg /= (hiBound - lowBound + 1);
            averages[i] = avg;
        }
    }
}
