// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class PlacePinAR : MonoBehaviour
{
    private ARRaycastManager raycastManager;
    private Vector2 screenCenter;
    private bool startPlacement = false;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        screenCenter = new Vector2(Screen.width/2, Screen.height/2);
    }

    public GameObject spawnObj;

    private Vector3 pinPos;
    private bool foundPlane = false;
    private void Update()
    {
        if (!startPlacement) return;
        
#if UNITY_ANDROID
        var touchPosition = new Vector2(Screen.width/2, Screen.height/2);
        if (raycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
        {
            var hitPose = hits[0].pose;
            pinPos = hitPose.position;
            if (!foundPlane) 
            {
                foundPlane = true;
                UIManager.Instance.SetDebugText("Tap to place pin");
            }
        }
#endif

        if (InputManager.Instance.DidClick())
        {
            startPlacement = false;
            var pin = GameObject.Instantiate(spawnObj, pinPos, Quaternion.identity);
            pin.name = "Test pin";
            OnClick?.Invoke(pin.transform);
        }
    }

    private Action<Transform> OnClick;
    public void StartPlacement(Action<Transform> onClick)
    {
        OnClick = onClick;
        startPlacement = true;
        foundPlane = false;
        UIManager.Instance.SetDebugText("Detecting surfaces.. Move device around");
    }
}
