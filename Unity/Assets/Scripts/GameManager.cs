// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

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
    Author,
    User
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

    private void Start()
    {
        SwitchAppMode(AppState.Start);
    }

    private AppState currState = AppState.Empty;
    private UserRole currRole;

    public void SwitchAppMode(AppState newState, Object data = null)
    {
        Debug.LogFormat("Switch from {0} to {1}", currState.ToString(), newState.ToString());
        currState = newState;

        switch(currState)
        {
            case AppState.Start:
                {
                    UIManager.Instance.ShowUserRoleSelection();
                    break;
                }
            case AppState.AnchorScanning:
                {
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
