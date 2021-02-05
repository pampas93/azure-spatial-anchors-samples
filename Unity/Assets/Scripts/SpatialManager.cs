// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors.Unity.Examples;

public class SpatialManager : MonoBehaviour
{
    [SerializeField] private SpatialAnchorManager CloudManager;

    private AnchorLocateCriteria anchorLocateCriteria = null;
    private CloudSpatialAnchor currentCloudAnchor;
    private bool isErrorActive = false;
    private CloudSpatialAnchorWatcher currentWatcher;
    private readonly List<string> anchorIdsToLocate = new List<string>();

    public async Task SetupStartSession()
    {
        Debug.Log("Sanity check: " + SanityCheckAccessConfiguration());
        if (CloudManager.Session == null)
        {
            await CloudManager.CreateSessionAsync();
        }
        currentCloudAnchor = null;

        if (!GameManager.Instance.IsAuthor)
        {
            var anchorList = AnchorUtils.GetSavedAnchorIdentifiers();
            ShowStatus($"Ready to locate {anchorList.Count} anchors");
            SetAnchorIdsToLocate(anchorList);
        }

        await CloudManager.StartSessionAsync();
        ShowStatus("Cloud Session start status: " + CloudManager.IsSessionStarted);
    }


    public virtual bool SanityCheckAccessConfiguration()
    {
        if (string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountId)
            || string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountKey)
            || string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountDomain))
        {
            return false;
        }

        return true;
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

#region Callbacks from CloudManager
    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        Debug.LogFormat("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
        if (args.Status == LocateAnchorStatus.Located) 
        {
            var newAnchor = args.Anchor;

            UnityDispatcher.InvokeOnAppThread(() =>
            {
                Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                anchorPose = newAnchor.GetPose();
#endif
                SpawnNewAnchoredObject(anchorPose.position, anchorPose.rotation, newAnchor);
            });
        }
    }

    private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
    }

    private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
        Debug.Log("Session updated " + args.Status.ToString());
    }

    private void CloudManager_Error(object sender, SessionErrorEventArgs args)
    {
        isErrorActive = true;
        Debug.Log(args.ErrorMessage);

        ShowStatus(string.Format("Error: {0}", args.ErrorMessage));
    }

    private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
    {
        Debug.Log(args.Message);
    }
#endregion

    public void CreateWatcher()
    {
        ShowStatus("Creating watcher");
        if (currentWatcher != null)
        {
            currentWatcher.Stop();
            currentWatcher = null;
        }

        try
        {
            if ((CloudManager != null) && (CloudManager.Session != null))
            {
                currentWatcher = CloudManager.Session.CreateWatcher(anchorLocateCriteria);
                ShowStatus("Session watcher created succesfully");
            }
            else
            {
                currentWatcher = null;
                throw new Exception("CloudManager not setup yet. Cannot create watcher");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            ShowStatus(ex.Message);
        }
    }

    /// <summary>
    /// Saves the current object anchor to the cloud; and returns the anchor identifier
    /// </summary>
    public async Task<string> SaveCurrentObjectAnchorToCloudAsync(GameObject pin)
    {
        try
        {
            // Get the cloud-native anchor behavior
            CloudNativeAnchor cna = pin.GetComponent<CloudNativeAnchor>();

            // If the cloud portion of the anchor hasn't been created yet, create it
            if (cna.CloudAnchor == null) { cna.NativeToCloud(); }
            // ShowStatus("Got cloud anchor");

            // Get the cloud portion of the anchor
            CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;

            // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
            cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

            while (!CloudManager.IsReadyForCreate)
            {
                await Task.Delay(330);
                float createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
                ShowStatus($"Move your device to capture more environment data: {createProgress:0%}");
            }

            bool success = false;
            ShowStatus("Saving...");

            try
            {
                // Actually save
                await CloudManager.CreateAnchorAsync(cloudAnchor);

                // Store
                currentCloudAnchor = cloudAnchor;

                // Success?
                success = currentCloudAnchor != null;

                if (success && !isErrorActive)
                {
                    ShowStatus("Saved success");
                    // Await override, which may perform additional tasks
                    // such as storing the key in the AnchorExchanger
                    return OnSaveCloudAnchorSuccessfulAsync(pin);
                }
                else
                {
                    OnSaveCloudAnchorFailed(new Exception("Failed to save, but no exception was thrown."));
                    return null;
                }
            }
            catch (Exception ex)
            {
                OnSaveCloudAnchorFailed(ex);
                return null;
            }
        }
        catch (Exception exe) 
        {
            ShowStatus(exe.Message);
            return null;
        }
    }

    public async Task DeleteAnchorAndCleanUp(GameObject pin)
    {
        var cna = pin.GetComponent<CloudNativeAnchor>();
        if (cna != null && cna.CloudAnchor != null)
        {
            await CloudManager.DeleteAnchorAsync(cna.CloudAnchor);
        }
        Destroy(pin);
    }

    // On success, returns the anchor ID
    private string OnSaveCloudAnchorSuccessfulAsync(GameObject newPin)
    {
        ShowStatus("Anchor created, yay!");
        
        // Sanity check that the object is still where we expect
        Pose anchorPose = Pose.identity;

        #if UNITY_ANDROID || UNITY_IOS
        anchorPose = currentCloudAnchor.GetPose();
        #endif
        // HoloLens: The position will be set based on the unityARUserAnchor that was located.

        MoveAnchoredObject(newPin, anchorPose.position, anchorPose.rotation, currentCloudAnchor);

        return currentCloudAnchor.Identifier;
    }

    private void SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor)
    {
        ShowStatus("Found and Spawing new anchor");
        // Create the prefab
        GameObject newPin = GameObject.Instantiate(
            GameManager.Instance.SpawnObj, worldPos, worldRot);

        // Attach a cloud-native anchor behavior to help keep cloud
        // and native anchors in sync.
        newPin.AddComponent<CloudNativeAnchor>();
    }

    private void MoveAnchoredObject(GameObject objectToMove, Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor = null)
    {
        // Get the cloud-native anchor behavior
        CloudNativeAnchor cna = objectToMove.GetComponent<CloudNativeAnchor>();

        // Warn and exit if the behavior is missing
        if (cna == null)
        {
            Debug.LogWarning($"The object {objectToMove.name} is missing the {nameof(CloudNativeAnchor)} behavior.");
            return;
        }

        // Is there a cloud anchor to apply
        if (cloudSpatialAnchor != null)
        {
            // Yes. Apply the cloud anchor, which also sets the pose.
            cna.CloudToNative(cloudSpatialAnchor);
        }
        else
        {
            // No. Just set the pose.
            cna.SetPose(worldPos, worldRot);
        }
    }

    private void ShowStatus(string text)
    {
        UnityDispatcher.InvokeOnAppThread(() => {
            UIManager.Instance.SetDebugText(text);
        });
    }

    private void OnSaveCloudAnchorFailed(Exception exception)
    {
        // we will block the next step to show the exception message in the UI.
        isErrorActive = true;
        Debug.LogException(exception);
        Debug.Log("Failed to save anchor " + exception.ToString());

        UnityDispatcher.InvokeOnAppThread(() => {
            UIManager.Instance.SetDebugText(string.Format("Error: {0}", exception.Message));
        });
    }

    private void SetAnchorIdsToLocate(IEnumerable<string> anchorIds)
    {
        if (anchorIds == null)
        {
            throw new ArgumentNullException(nameof(anchorIds));
        }

        anchorIdsToLocate.Clear();
        anchorIdsToLocate.AddRange(anchorIds);

        anchorLocateCriteria.Identifiers = anchorIdsToLocate.ToArray();
    }
}
