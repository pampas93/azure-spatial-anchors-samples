// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SavePinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text anchorID;
    [SerializeField] private TMP_InputField notes;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject imageCaptureUI;
    [SerializeField] private TMP_Text recordBtnText;

    Action<AnchorData> onSave;
    Action onCancel;

    private AnchorData currAnchor;

    // Called when the "Save" button is clicked in the Anchor data entry UI
    public void OnSaveClick()
    {
        currAnchor.SetNotes(notes.text);
        onSave?.Invoke(currAnchor);
        ClearSaveUI();
    }

    // Called when the "Cancel" button is clicked. Cancels the save order and deletes the anchor
    public void OnCancelClick()
    {
        onCancel?.Invoke();
        ClearSaveUI();
    }

    public void ShowSaveAnchorUI(string id, Action<AnchorData> onSave, Action onCancel)
    {
        gameObject.SetActive(true);
        this.onSave = onSave;
        this.onCancel = onCancel;

        currAnchor = new AnchorData(id);
        anchorID.text = id;
    }

    private void ClearSaveUI()
    {
        currAnchor = null;
        anchorID.text = "";
        notes.text = "";
        gameObject.SetActive(false);
    }

#region Recording UI functions
    bool isRecording = false;
    public bool IsRecording {
        get => isRecording;
        set {
            isRecording = value;
            recordBtnText.text = isRecording ? "Stop Record" : "Start Record";
        }
    }

    public void StartStopRecord()
    {
        if (IsRecording)
        {
            bool res = AudioManager.Instance.StopSaveRecording(currAnchor.GetAudioPath());
            UIManager.Instance.SetDebugText(res ? "Saved recording" : "Oops, failed to save recording");
            IsRecording = false;
        }
        else
        {
            IsRecording = true;
            UIManager.Instance.SetDebugText("Recording started");
            AudioManager.Instance.StartRecording();
        }
    }

    public void ClearAudio()
    {
        currAnchor.DeleteAudio();
    }
#endregion

#region Capture Image functions
    public void StartCamera()
    {
        imageCaptureUI.SetActive(true);
        CapturePhoto.Instance.StartCamera();
        parent.SetActive(false);
    }

    public void CaptureImage()
    {
        CapturePhoto.Instance.Capture(currAnchor.GetImagePath(), 
            () => {
                Debug.Log("Pemp Image file exists? : " + System.IO.File.Exists(currAnchor.GetImagePath()));
                Debug.Log("Pemp Size: " + (new System.IO.FileInfo(currAnchor.GetImagePath())).Length);
                CloseCameraView();
            });
    }

    public void CloseCameraView()
    {
        imageCaptureUI.SetActive(false);
        parent.SetActive(true);
    }

    public void ClearImage()
    {
        currAnchor.DeleteImage();
    }
#endregion
}
