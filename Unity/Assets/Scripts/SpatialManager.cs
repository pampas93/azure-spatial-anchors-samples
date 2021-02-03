// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors.Unity.Examples;

public class SpatialManager : MonoBehaviour
{
    [SerializeField] private SpatialAnchorManager CloudManager;

    private AnchorLocateCriteria anchorLocateCriteria = null;
    private CloudSpatialAnchor currentCloudAnchor;

    public async void SetupStartSession()
    {
        if (CloudManager.Session == null)
        {
            await CloudManager.CreateSessionAsync();
        }
        currentCloudAnchor = null;

        if (!GameManager.Instance.IsAuthor)
        {
            // Todo (abhi)
            // ConfigureSession with all anchor ids
        }

        await CloudManager.StartSessionAsync();
    }

    private void Start()
    {
        CloudManager.SessionUpdated += CloudManager_SessionUpdated;
        CloudManager.AnchorLocated += CloudManager_AnchorLocated;
        CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        CloudManager.LogDebug += CloudManager_LogDebug;
        CloudManager.Error += CloudManager_Error;

        anchorLocateCriteria = new AnchorLocateCriteria();
    }

    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        Debug.LogFormat("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
        if (args.Status == LocateAnchorStatus.Located)
        {
        }
    }

    private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
    }

    private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
    }

    private void CloudManager_Error(object sender, SessionErrorEventArgs args)
    {
        Debug.Log(args.ErrorMessage);

        UnityDispatcher.InvokeOnAppThread(() => {
            UIManager.Instance.SetDebugText(string.Format("Error: {0}", args.ErrorMessage));
        });
    }

    private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
    {
        Debug.Log(args.Message);
    }
}
