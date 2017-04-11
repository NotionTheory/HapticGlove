using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BeatDetector))]
public class BeatDetectorEditor : Editor
{
    DateTime lastSave = DateTime.MinValue;
    string lastResourceName;
    public override void OnInspectorGUI()
    {
        var beatDetector = (BeatDetector)target;
        var config = beatDetector.Configuration; 
        var fileName = Path.GetFullPath(string.Format("Assets/Resources/{0}.json", beatDetector.resourceName));

        if(beatDetector.resourceName != lastResourceName)
        {
            config.LoadAudioMetadata(beatDetector.resourceName);
        }

        DrawDefaultInspector();

        var sinceLastSave = DateTime.Now - lastSave;
        if(sinceLastSave.TotalSeconds <= 2)
        {
            GUILayout.Label("Saved!");
        }
        else if(GUILayout.Button("Save settings"))
        {
            var json = JsonUtility.ToJson(config, true);
            File.WriteAllText(fileName, json);
            lastSave = DateTime.Now;
        }
        else if(GUILayout.Button("Reload settings"))
        {
            config.LoadAudioMetadata(beatDetector.resourceName);
        }

        lastResourceName = beatDetector.resourceName;
    }
}
