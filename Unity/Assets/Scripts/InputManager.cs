// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { 
        get {
            return instance;
        } 
    }

    private static InputManager instance;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit))
            {
                var clickComp = raycastHit.collider.GetComponent<MarkerClick>();
                if (clickComp != null)
                {
                    clickComp.OnClick();
                }
            }
        }
    }

    public bool DidClick()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }
#else
        for (var i = 0; i < Input.touchCount; ++i) 
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began) 
            {
                return true;
            }
        }
#endif
        return false;
    }
}
