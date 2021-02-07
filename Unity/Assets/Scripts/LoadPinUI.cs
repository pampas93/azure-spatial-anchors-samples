// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text anchorID;
    [SerializeField] private TMP_Text notes;
    [SerializeField] private Button audioPlayBtn;

    private AnchorData currAnchor;

    // Called when the "Cancel" button is clicked. Cancels the save order and deletes the anchor
    public void OnCloseClick()
    {
        gameObject.SetActive(false);
        anchorID.text = "";
        notes.text = "";
        AudioManager.Instance.ClearAudioClip();
    }

    public void LoadAnchorUI(AnchorData data)
    {
        gameObject.SetActive(true);
        currAnchor = data;
        anchorID.text = currAnchor.ID;
        notes.text = currAnchor.GetNotes();

        audioPlayBtn.interactable = currAnchor.DoesAudioExist();
    }

    public void PlayRecording()
    {
        AudioManager.Instance.PlayRecording(currAnchor.GetAudioPath());
    }
}
