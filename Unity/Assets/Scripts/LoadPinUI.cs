// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPinUI : MonoBehaviour
{
    [SerializeField] private GameObject parent;

    [SerializeField] private TMP_Text anchorID;
    [SerializeField] private TMP_Text notes;
    [SerializeField] private Button imageShowBtn;
    [SerializeField] private Button audioPlayBtn;

    [Header("Image window")]
    [SerializeField] private GameObject window;
    [SerializeField] private Image imageWindow;

    private AnchorData currAnchor;

    // Called when the "Cancel" button is clicked. Cancels the save order and deletes the anchor
    public void OnCloseClick()
    {
        gameObject.SetActive(false);
        anchorID.text = "";
        notes.text = "";
        CloseImage();
        AudioManager.Instance.ClearAudioClip();
    }

    public void LoadAnchorUI(AnchorData data)
    {
        Debug.Log("LoadAnchorUI");
        Debug.Log(data);
        gameObject.SetActive(true);
        currAnchor = data;
        anchorID.text = currAnchor.ID;
        notes.text = currAnchor.GetNotes();

        imageShowBtn.interactable = currAnchor.DoesImageExist();
        audioPlayBtn.interactable = currAnchor.DoesAudioExist();
    }

    public void PlayRecording()
    {
        AudioManager.Instance.PlayRecording(currAnchor.GetAudioPath());
    }

    public void ShowImage()
    {
        Sprite image = CapturePhoto.Instance.GetImageSprite(currAnchor.GetImagePath());
        if (image != null)
        {
            window.SetActive(true);
            parent.SetActive(false);
            imageWindow.sprite = image;
        }
        else
        {
            UIManager.Instance.SetDebugText("Unable to show image. sorry");
        }
    }

    public void CloseImage()
    {
        imageWindow.sprite = null;
        window.SetActive(false);
        parent.SetActive(true);
    }
}
