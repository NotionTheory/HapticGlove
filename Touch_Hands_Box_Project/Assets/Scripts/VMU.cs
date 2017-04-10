using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VMU : MonoBehaviour
{
    public Transform Prefab;
    public Vector3 BarScale = new Vector3(0.1f, 4f, 0.1f);

    List<Transform> bars = new List<Transform>();

    public void SetVMU(float[] spectrum)
    {
        while(bars.Count < spectrum.Length)
        {
            var bar = Instantiate(Prefab);
            bar.SetParent(transform);
            bars.Add(bar);
        }

        while(bars.Count > spectrum.Length)
        {
            var last = bars.Count - 1;
            var bar = bars[last];
            bars.RemoveAt(last);
            Destroy(bar.gameObject);
        }

        var offset = 0.5f * (bars.Count - 1);
        for(int i = 0; i < bars.Count; ++i)
        {
            var bar = bars[i];
            var scale = BarScale;
            scale.y *= spectrum[i];
            bar.localScale = scale;
            bar.transform.localPosition = new Vector3(BarScale.x * (i - offset), 0, 0);
        }
    }
}
