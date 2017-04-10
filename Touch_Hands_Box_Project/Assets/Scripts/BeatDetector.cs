using System;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class OnSpectrumEventHandler : UnityEngine.Events.UnityEvent<float[]>
{

}

public class BeatDetector : MonoBehaviour
{
    public AudioSource Source;

    [Range(6, 13)]
    public int BufferMagnitude = 10;
    [Range(0, 1)]
    public float Sensitivity = 0.1f;
    [Range(1, 100)]
    public int BandCount = 24;
    [Range(10, 200)]
    public int RingBufferSize = 120;

    [Range(1, 32)]
    public int blipDelayLen = 16;
    int[] blipDelay;

    [Range(0, 1024)]
    public int BandPassLow = 0;
    [Range(0, 1024)]
    public int BandPassHigh = 1024;
    public bool ViewSpectrumBeforeBandpass = true;

    int updatesSinceLastBeat = 0;

    float[] frequencyDomainSlice;
    float[] binnedSpectrum;
    float[] acVals;
    float[] onsets;
    float[] beatScores;
    bool[] didBeat;
    int ringBufferIndex = 0;

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

    [Header ("Events")]
    public UnityEngine.Events.UnityEvent OnBeat;
    public OnSpectrumEventHandler OnSpectrum;

    void Start()
    {
        Source = FindObjectOfType<AudioSource>();
        lastT = CurrentTime;
    }

    private void InitArrays()
    {
        if(frequencyDomainSlice == null || frequencyDomainSlice.Length != BufferSize)
        {
            frequencyDomainSlice = new float[BufferSize];
        }

        if(binnedSpectrum == null || binnedSpectrum.Length != BandCount)
        {
            binnedSpectrum = new float[BandCount];
            binnedAmplitude = new float[BandCount];
        }

        if(onsets == null || RingBufferSize != onsets.Length)
        {
            onsets = new float[RingBufferSize];
            beatScores = new float[RingBufferSize];
            didBeat = new bool[RingBufferSize];
        }

        if(blipDelay == null || blipDelay.Length != blipDelayLen)
        {
            blipDelay = new int[blipDelayLen];
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
    }

    void Update()
    {
        InitArrays();
        if(Source.isPlaying)
        {
            ringBufferIndex %= RingBufferSize;
            Source.GetSpectrumData(frequencyDomainSlice, 0, FFTWindow.BlackmanHarris);
            
            BinSpectrum(ViewSpectrumBeforeBandpass);

            for(int i = 0; i < BufferSize; ++i)
            {
                if(i < BandPassLow || i > BandPassHigh)
                {
                    frequencyDomainSlice[i] = 0;
                }
            }

            BinSpectrum(!ViewSpectrumBeforeBandpass);

            // calculate the value of the onset function in this frame
            float onset = 0;
            for(int i = 0; i < BandCount; i++)
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
            for(int i = tempopd / 2; i < Mathf.Min(RingBufferSize, 2 * tempopd); ++i)
            {
                // objective function - this beat's cost + score to last beat + transition penalty
                float score = onset
                    + beatScores[(ringBufferIndex - i + RingBufferSize) % RingBufferSize]
                    - 100 * Sensitivity * Mathf.Pow(Mathf.Log((float)i / tempopd), 2);
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
            for(int i = 1; i < RingBufferSize; ++i)
            {
                if(beatScores[i] < minBeatScore)
                {
                    minBeatScore = beatScores[i];
                }
            }
            for(int i = 0; i < RingBufferSize; ++i)
            {
                beatScores[i] -= minBeatScore;
            }

            // find the largest value in the score fn window, to decide if we emit a blip
            maxBeatScore = beatScores[0];
            maxBeatScoreIndex = 0;
            for(int i = 0; i < RingBufferSize; ++i)
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

    private void BinSpectrum(bool emitEvent)
    {
        for(int i = 0; i < BandCount; i++)
        {
            float avg = 0;
            int lowFreq = (int)(SamplingRate / Mathf.Pow(2, BandCount - i + 1));
            int hiFreq = (int)(SamplingRate / Mathf.Pow(2, BandCount - i));
            int lowBound = FrequencyIndex(lowFreq);
            int hiBound = FrequencyIndex(hiFreq);
            for(int j = lowBound; j <= hiBound; j++)
            {
                avg += frequencyDomainSlice[j];
            }
            avg /= (hiBound - lowBound + 1);
            binnedSpectrum[i] = avg;
        }
        
        if(emitEvent && OnSpectrum != null)
        {
            OnSpectrum.Invoke(binnedSpectrum);
        }
    }

    int SamplingRate
    {
        get
        {
            return Source.clip.frequency;
        }
    }

    int BufferSize
    {
        get
        {
            return (int)Mathf.Pow(2, BufferMagnitude);
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
}
