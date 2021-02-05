// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors.Unity;

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
    [SerializeField] private GameObject spawnObj;

    public GameObject SpawnObj => spawnObj;

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
                    UIManager.Instance.SetDebugText("Show roles");
                    UIManager.Instance.ShowUserRoleSelection();
                    break;
                }
            case AppState.AnchorScanning:
                {
                    await spatialManager.SetupStartSession();
                    if (IsAuthor)
                    {
                        pinPlacement.StartPlacement(async (newPin) => {
                            var cna = newPin.gameObject.AddComponent<CloudNativeAnchor>();
                            // If the cloud portion of the anchor hasn't been created yet, create it
                            if (cna.CloudAnchor == null) { cna.NativeToCloud(); }
                            UIManager.Instance.SetDebugText(newPin.name);
                            await SwitchAppMode(AppState.Save, newPin);
                        });
                    }
                    else
                    {
                        if (AnchorUtils.GetSavedAnchorIdentifiers().Count < 1) 
                        {
                            UIManager.Instance.SetDebugText("No anchors to locate");
                        }
                        else
                        {
                            spatialManager.CreateWatcher();
                        }
                    }
                    
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
                        string anchorId = await Task.FromResult(spatialManager.SaveCurrentObjectAnchorToCloudAsync(newPin.gameObject)).Result;
                        if (string.IsNullOrEmpty(anchorId))
                        {
                            DeleteAnchor(newPin.gameObject);
                        }
                        else
                        {
                            UIManager.Instance.SetDebugText("Saving anchor with data internally");
                            // Anchor was saved succesfully on cloud
                            UIManager.Instance.ShowSaveAnchorUI(anchorId, (anchorData) => {
                                AnchorUtils.SaveAnchor(anchorData);
                                SwitchAppMode(AppState.AnchorScanning);
                            }, () => {
                                DeleteAnchor(newPin.gameObject);
                                SwitchAppMode(AppState.AnchorScanning);
                            });
                        }
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

        async void DeleteAnchor(GameObject pin)
        {
            UIManager.Instance.SetDebugText("Deleting anchor without save");
            await spatialManager.DeleteAnchorAndCleanUp(pin);
        }
    }

    public void OnRoleSelected(UserRole role)
    {
        currRole = role;
        SwitchAppMode(AppState.AnchorScanning);
    }

    public void OnAnchorClick(string id)
    {
        if (!IsAuthor)
        {
            var anchorData = AnchorUtils.GetAnchorData(id);
            if (anchorData == null)
            {
                UIManager.Instance.SetDebugText("Oops. Couldn't fetch anchor details");
                return;
            }
            UIManager.Instance.LoadAnchorData(anchorData);
        }
    }
}
