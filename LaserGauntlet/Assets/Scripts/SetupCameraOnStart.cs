using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupCameraOnStart : MonoBehaviour
{
    public Camera mainCamera;

	void Start ()
    {
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.farClipPlane = 1000.0f;
        SteamVR_Fade.View(Color.clear, 0.0f);
	}
	
	void Update ()
    {
		
	}
}
