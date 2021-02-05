// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using UnityEngine;
using TMPro;

public class LoadPinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text anchorID;
    [SerializeField] private TMP_Text notes;

    // Called when the "Cancel" button is clicked. Cancels the save order and deletes the anchor
    public void OnCloseClick()
    {
        gameObject.SetActive(false);
    }

    public void LoadAnchorUI(AnchorData data)
    {
        gameObject.SetActive(true);
        anchorID.text = data.ID;
        notes.text = string.IsNullOrEmpty(data.Notes) ? "No notes available" : data.Notes;
    }
}
