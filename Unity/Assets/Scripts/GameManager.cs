// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Collections;
using UnityEngine;

public enum AppState
{
    Start,
    AnchorScanning,
    AnchorSelect,
    Stop,
    Empty
}

public enum UserRole
{
    Author = 1,
    User = 2
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { 
        get {
            return instance;
        } 
    }
    private static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private SpatialManager spatialManager;
    [SerializeField] private PlacePinAR pinPlacement;

    private void Start()
    {
        SwitchAppMode(AppState.Start);
    }

    private AppState currState = AppState.Empty;
    private UserRole currRole;

    public AppState CurrentState => currState;
    public bool IsAuthor => currRole == UserRole.Author;

    /// <summary>
    /// Switch between states and execute state specific functions
    /// </summary>
    public void SwitchAppMode(AppState newState, System.Object data = null)
    {
        Debug.LogFormat("Switch from {0} to {1}", currState.ToString(), newState.ToString());
        currState = newState;

        switch(currState)
        {
            case AppState.Start:
                {
                    spatialManager.SetupStartSession();
                    UIManager.Instance.ShowUserRoleSelection();
                    break;
                }
            case AppState.AnchorScanning:
                {
                    if (IsAuthor)
                    {
                        pinPlacement.StartPlacement((newPin) => {
                            UIManager.Instance.SetDebugText(newPin.name);
                        });
                    }
                    Debug.Log("Started scanning");
                    break;
                }
            case AppState.AnchorSelect:
                {
                    break;
                }
            case AppState.Stop:
                {
                    break;
                }
            default:
                {
                    Debug.LogError("State not implemented");
                    break;
                }
        }
    }

    public void OnRoleSelected(UserRole role)
    {
        currRole = role;
        SwitchAppMode(AppState.AnchorScanning);
    }
}
