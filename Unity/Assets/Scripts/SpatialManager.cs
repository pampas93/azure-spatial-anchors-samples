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
            // Todo (abhi)
            // ConfigureSession with all anchor ids
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

    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        Debug.LogFormat("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
        if (args.Status == LocateAnchorStatus.Located) {}
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

    /// <summary>
    /// Saves the current object anchor to the cloud.
    /// </summary>
    public async Task SaveCurrentObjectAnchorToCloudAsync(GameObject pin)
    {
        try
        {
            // Get the cloud-native anchor behavior
            CloudNativeAnchor cna = pin.GetComponent<CloudNativeAnchor>();
            ShowStatus("GetComponent success");

            // If the cloud portion of the anchor hasn't been created yet, create it
            if (cna.CloudAnchor == null) { cna.NativeToCloud(); }
            ShowStatus("Got cloud anchor");

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
                    OnSaveCloudAnchorSuccessfulAsync(pin);
                }
                else
                {
                    OnSaveCloudAnchorFailed(new Exception("Failed to save, but no exception was thrown."));
                }
            }
            catch (Exception ex)
            {
                OnSaveCloudAnchorFailed(ex);
            }
        }
        catch (Exception exe) 
        {
            ShowStatus(exe.Message);
        }
    }

    string currentAnchorId = "";

    private void OnSaveCloudAnchorSuccessfulAsync(GameObject newPin)
    {
        ShowStatus("Anchor created, yay!");
        
        currentAnchorId = currentCloudAnchor.Identifier;
        SaveToFile();

        // Sanity check that the object is still where we expect
        Pose anchorPose = Pose.identity;

        #if UNITY_ANDROID || UNITY_IOS
        anchorPose = currentCloudAnchor.GetPose();
        #endif
        // HoloLens: The position will be set based on the unityARUserAnchor that was located.

        SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation, newPin);
    }

    private void SpawnOrMoveCurrentAnchoredObject(Vector3 worldPos, Quaternion worldRot, GameObject newPin)
    {
        // // Create the object if we need to, and attach the platform appropriate
        // // Anchor behavior to the spawned object
        // if (spawnedObject == null)
        // {
        //     // Use factory method to create
        //     spawnedObject = SpawnNewAnchoredObject(worldPos, worldRot, currentCloudAnchor);

        //     // Update color
        //     spawnedObjectMat = spawnedObject.GetComponent<MeshRenderer>().material;
        // }
        // else
        // {
        //     // Use factory method to move
        //     MoveAnchoredObject(spawnedObject, worldPos, worldRot, currentCloudAnchor);
        // }

        MoveAnchoredObject(newPin, worldPos, worldRot, currentCloudAnchor);
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

    protected virtual void OnSaveCloudAnchorFailed(Exception exception)
    {
        // we will block the next step to show the exception message in the UI.
        isErrorActive = true;
        Debug.LogException(exception);
        Debug.Log("Failed to save anchor " + exception.ToString());

        UnityDispatcher.InvokeOnAppThread(() => {
            UIManager.Instance.SetDebugText(string.Format("Error: {0}", exception.Message));
        });
    }

    private void SaveToFile()
    {
        var path = Path.Combine(Application.persistentDataPath, "anchor.txt");
        
        if (File.Exists(path)) {
            File.Delete(path);
        }

        File.Create(path).Dispose();
        File.WriteAllText(path, currentAnchorId);
    }
}
