using System;
using System.IO;
using UnityEngine;

/*
 * Beat Detector script. 
 * Add it as a component to anywhere you like. 
 * Drag onto it your AudioSource. 
 * Set the Events for OnBeat and OnSpectrum (data for visualization). 
 * Fiddle with the sliders (probably stay away from "Other", I don't really understand them yet). 
 * Click the "Save settings" button to save a config file to the Resources directory.
 */
[Serializable]
public class OnSpectrumEventHandler : UnityEngine.Events.UnityEvent<float[]>
{

}

[Serializable]
public class BeatDetectorConfiguration
{
    [Header("FFT")]
    [Range(6, 13)]
    public int BufferMagnitude = 10;
    [Range(1, 100)]
    public int BandCount = 24;

    [Header("Band Pass")]
    [Range(0, 1024)]
    public int BandPassLow = 0;
    [Range(0, 1024)]
    public int BandPassHigh = 1024;

    [Header("Other")]
    [Range(10, 200)]
    public int RingBufferSize = 120;
    [Range(1, 32)]
    public int BlipDelayLen = 16;
    [Range(0, 1)]
    public float Sensitivity = 0.1f;

    public void Copy(BeatDetectorConfiguration toCopy)
    {
        var t = typeof(BeatDetectorConfiguration);
        var fields = t.GetFields();
        foreach(var field in fields)
        {
            field.SetValue(this, field.GetValue(toCopy));
        }
    }

    public void LoadAudioMetadata(string resourceName)
    {
        var res = Resources.Load<TextAsset>(resourceName);
        if(res != null)
        {
            var config = JsonUtility.FromJson<BeatDetectorConfiguration>(res.text);
            this.Copy(config);
        }
        else
        {
            Debug.LogFormat("Couldn't find resource {0}", resourceName);
        }
    }
}

public class BeatDetector : MonoBehaviour
{
    public AudioSource Source;

    [Header ("Events")]
    public UnityEngine.Events.UnityEvent OnBeat;
    public OnSpectrumEventHandler OnSpectrum;

    public bool ViewSpectrumBeforeBandpass = true;

    [Range(1, 32)]
    public int VisualizationBandCount = 16;

    public BeatDetectorConfiguration Configuration;

    int updatesSinceLastBeat = 0;

    float[] frequencyDomainSlice;
    float[] visualizationBinnedSpectrum;
    float[] binnedSpectrum;
    float[] acVals;
    float[] onsets;
    float[] beatScores;
    bool[] didBeat;
    int ringBufferIndex = 0;
    int[] blipDelay;

    float tempo, lastT, nowT, diff, entries, sum;

    float[] binnedAmplitude;
    // the spectrum of the previous step

    // Autocorrelation structure
    int maxlag = 100;
    // (in frames) largest lag to track
    float decay = 0.997f;
    float lastBandWidth;
    // smoothing constant for running average

    float[] delays;
    float[] outputs;
    int indx;

    float[] bpms;
    float[] rweight;
    float BPMMidPointWeight = 120f;

    string lastResourceName;

    void Start()
    {
        Source = FindObjectOfType<AudioSource>();
        lastT = CurrentTime;
    }

    public string resourceName
    {
        get
        {
            return "AudioMetadata/" + Source.clip.name;
        }
    }

    private void InitArrays()
    {
        if(resourceName != lastResourceName)
        {
            Configuration.LoadAudioMetadata(resourceName);
        }

        if(frequencyDomainSlice == null || frequencyDomainSlice.Length != BufferSize)
        {
            frequencyDomainSlice = new float[BufferSize];
        }

        if(visualizationBinnedSpectrum == null || visualizationBinnedSpectrum.Length != VisualizationBandCount)
        {
            visualizationBinnedSpectrum = new float[VisualizationBandCount];
        }

        if(binnedSpectrum == null || binnedSpectrum.Length != Configuration.BandCount)
        {
            binnedSpectrum = new float[Configuration.BandCount];
            binnedAmplitude = new float[Configuration.BandCount];
        }

        if(onsets == null || Configuration.RingBufferSize != onsets.Length)
        {
            onsets = new float[Configuration.RingBufferSize];
            beatScores = new float[Configuration.RingBufferSize];
            didBeat = new bool[Configuration.RingBufferSize];
        }

        if(blipDelay == null || blipDelay.Length != Configuration.BlipDelayLen)
        {
            blipDelay = new int[Configuration.BlipDelayLen];
        }

        bool recalcWeights = false;
        if(acVals == null || acVals.Length != maxlag)
        {
            acVals = new float[maxlag];
            delays = new float[maxlag];
            outputs = new float[maxlag];
            bpms = new float[maxlag];
            rweight = new float[maxlag];
            recalcWeights = true;
        }

        if(recalcWeights || BandWidth != lastBandWidth)
        {
            // calculate a log-lag gaussian weighting function, to prefer tempi around 120 bpm
            for(int i = 0; i < maxlag; ++i)
            {
                bpms[i] = BandWidth * 60.0f / i;
                // weighting is Gaussian on log-BPM axis, centered at wmidbpm, SD = woctavewidth octaves
                rweight[i] = Mathf.Exp(-0.5f * Mathf.Pow(
                    Mathf.Log(bpms[i] / BPMMidPointWeight) /
                    (Mathf.Log(2.0f) * BandWidth),
                    2.0f));
            }
        }
        lastBandWidth = BandWidth;
        lastResourceName = resourceName;
    }

    void Update()
    {
        InitArrays();
        if(Source.isPlaying)
        {
            ringBufferIndex %= Configuration.RingBufferSize;
            Source.GetSpectrumData(frequencyDomainSlice, 0, FFTWindow.BlackmanHarris);

            if(ViewSpectrumBeforeBandpass)
            {
                BinSpectrum(true, visualizationBinnedSpectrum);
            }

            for(int i = 0; i < BufferSize; ++i)
            {
                if(i < Configuration.BandPassLow || i > Configuration.BandPassHigh)
                {
                    frequencyDomainSlice[i] = 0;
                }
            }

            BinSpectrum(!ViewSpectrumBeforeBandpass, binnedSpectrum);

            // calculate the value of the onset function in this frame
            float onset = 0;
            for(int i = 0; i < Configuration.BandCount; i++)
            {
                float bandDecibels = 0.025f * Mathf.Max(-100.0f, 20.0f * Mathf.Log10(binnedSpectrum[i]) + 160); // dB value of this band
                float deltaBandDecibels = bandDecibels - binnedAmplitude[i]; // dB increment since last frame
                binnedAmplitude[i] = bandDecibels; // record this frame to use next time around
                onset += deltaBandDecibels; // onset function is the sum of dB increments
            }

            onsets[ringBufferIndex] = onset;

            // update autocorrelator and find peak lag = current tempo
            delays[indx] = onset;

            // update running autocorrelator values
            for(int i = 0; i < maxlag; ++i)
            {
                int delix = (indx - i + maxlag) % maxlag;
                outputs[i] += (1 - decay) * (delays[indx] * delays[delix] - outputs[i]);
            }

            indx = (indx + 1) % maxlag;
            // record largest value in (weighted) autocorrelation as it will be the tempo
            float aMax = 0.0f;
            int tempopd = 0;

            for(int i = 0; i < maxlag; ++i)
            {
                float acVal = Mathf.Sqrt(rweight[i] * outputs[i]);
                if(acVal > aMax)
                {
                    aMax = acVal;
                    tempopd = i;
                }
                // store in array backwards, so it displays right-to-left, in line with traces
                acVals[maxlag - 1 - i] = acVal;
            }

            // calculate DP-ish function to update the best-score function
            float maxBeatScore = -999999;
            int maxBeatScoreIndex = 0;
            // consider all possible preceding beat times from 0.5 to 2.0 x current tempo period
            for(int i = tempopd / 2; i < Mathf.Min(Configuration.RingBufferSize, 2 * tempopd); ++i)
            {
                // objective function - this beat's cost + score to last beat + transition penalty
                float score = onset
                    + beatScores[(ringBufferIndex - i + Configuration.RingBufferSize) % Configuration.RingBufferSize]
                    - 100 * Configuration.Sensitivity * Mathf.Pow(Mathf.Log((float)i / tempopd), 2);
                // keep track of the best-scoring predecesor
                if(score > maxBeatScore)
                {
                    maxBeatScore = score;
                    maxBeatScoreIndex = i;
                }
            }

            beatScores[ringBufferIndex] = maxBeatScore;
            // keep the smallest value in the score fn window as zero, by subtracing the min val
            float minBeatScore = beatScores[0];
            for(int i = 1; i < Configuration.RingBufferSize; ++i)
            {
                if(beatScores[i] < minBeatScore)
                {
                    minBeatScore = beatScores[i];
                }
            }
            for(int i = 0; i < Configuration.RingBufferSize; ++i)
            {
                beatScores[i] -= minBeatScore;
            }

            // find the largest value in the score fn window, to decide if we emit a blip
            maxBeatScore = beatScores[0];
            maxBeatScoreIndex = 0;
            for(int i = 0; i < Configuration.RingBufferSize; ++i)
            {
                if(beatScores[i] > maxBeatScore)
                {
                    maxBeatScore = beatScores[i];
                    maxBeatScoreIndex = i;
                }
            }

            // dobeat array records where we actally place beats
            didBeat[ringBufferIndex] = false;  // default is no beat this frame
            ++updatesSinceLastBeat;
            // if current value is largest in the array, probably means we're on a beat
            if(maxBeatScoreIndex == ringBufferIndex && updatesSinceLastBeat > tempopd / 4)
            {
                this.tempo = TapTempo() * 1000;
                // make sure the most recent beat wasn't too recently
                if(OnBeat != null)
                {
                    OnBeat.Invoke();
                }
                blipDelay[0] = 1;
                // record that we did actually mark a beat this frame
                didBeat[ringBufferIndex] = true;
                // reset counter of frames since last beat
                updatesSinceLastBeat = 0;
            }

            // update column index (for ring buffer)
            ++ringBufferIndex;
        }
    }

    int FrequencyIndex(int freq)
    {
        if(freq < BandWidth / 2)
        {
            return 0;
        }
        else if(freq > (SamplingRate - BandWidth) / 2)
        {
            return BufferSize / 2;
        }
        else
        {
            float fraction = (float)freq / SamplingRate;
            int i = (int)Mathf.Round(BufferSize * fraction);
            return i;
        }
    }

    private void BinSpectrum(bool emitEvent, float[] arr)
    {
        for(int i = 0; i < arr.Length; i++)
        {
            float avg = 0;
            int n = 0;
            int lowFreq = (int)(SamplingRate / Mathf.Pow(2, arr.Length - i + 1));
            int hiFreq = (int)(SamplingRate / Mathf.Pow(2, arr.Length - i));
            int lowBound = FrequencyIndex(lowFreq);
            int hiBound = FrequencyIndex(hiFreq);
            for(int j = lowBound; j <= hiBound; j++)
            {
                avg += frequencyDomainSlice[j];
            }
            avg /= (hiBound - lowBound + 1);
            arr[i] = avg;
        }

        if(emitEvent && OnSpectrum != null)
        {
            OnSpectrum.Invoke(arr);
        }
    }

    int SamplingRate
    {
        get
        {
            return Source.clip.frequency;
        }
    }


    private int _bufferSize, _lastBufferMagnitude = -1;
    int BufferSize
    {
        get
        {
            if(Configuration.BufferMagnitude != _lastBufferMagnitude)
            {
                _bufferSize = (int)Mathf.Pow(2, Configuration.BufferMagnitude);
                _lastBufferMagnitude = Configuration.BufferMagnitude;
            }
            return _bufferSize;
        }
    }

    float BandWidth
    {
        get
        {
            return (float)SamplingRate / BufferSize;
        }
    }

    float TapTempo()
    {
        nowT = CurrentTime;
        diff = nowT - lastT;
        lastT = nowT;
        sum = sum + diff;
        entries++;

        return sum / entries;
    }

    public float CurrentTime
    {
        get
        {
            return Source.time;
        }
    }
}
