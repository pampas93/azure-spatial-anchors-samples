// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors.Unity;
using System.IO;

public enum AppState
{
    Start,
    AnchorScanning,
    AnchorSelect,
    Save,
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
        AnchorUtils.Setup();
    }

    private AppState currState = AppState.Empty;
    private UserRole currRole;

    public AppState CurrentState => currState;
    public bool IsAuthor => currRole == UserRole.Author;

    /// <summary>
    /// Switch between states and execute state specific functions
    /// </summary>
    public async Task SwitchAppMode(AppState newState, object data = null)
    {
        Debug.LogFormat("Switch from {0} to {1}", currState.ToString(), newState.ToString());
        currState = newState;

        switch(currState)
        {
            case AppState.Start:
                {
                    await spatialManager.SetupStartSession();
                    UIManager.Instance.SetDebugText("Show roles");
                    UIManager.Instance.ShowUserRoleSelection();
                    break;
                }
            case AppState.AnchorScanning:
                {
                    if (IsAuthor)
                    {
                        pinPlacement.StartPlacement(async (newPin) => {
                            newPin.gameObject.AddComponent<CloudNativeAnchor>();
                            UIManager.Instance.SetDebugText(newPin.name);
                            await SwitchAppMode(AppState.Save, newPin);
                        });
                    }
                    Debug.Log("Started scanning");
                    break;
                }
            case AppState.AnchorSelect:
                {
                    break;
                }
            case AppState.Save:
                {
                    if (data is Transform newPin)
                    {
                        UIManager.Instance.SetDebugText("Saving pin " + newPin.name);
                        await spatialManager.SaveCurrentObjectAnchorToCloudAsync(newPin.gameObject);
                    }
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

    // public async void AdvanceDemo()
    // {
    //     try
    //     {
    //         var advanceDemoTask = SwitchAppMode(AppState.Save);
    //         await advanceDemoTask;
    //     }
    //     catch (Exception ex)
    //     {
    //         // Debug.LogError($"{nameof(DemoScriptBase)} - Error in {nameof(AdvanceDemo)}: {ex.Message} {ex.StackTrace}");
    //         print($"Demo failed, check debugger output for more information");
    //     }
    // }
}
