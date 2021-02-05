// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using UnityEngine;
using TMPro;

public class SavePinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text anchorID;
    [SerializeField] private TMP_InputField notes;

    Action<AnchorData> onSave;
    Action onCancel;

    // Called when the "Save" button is clicked in the Anchor data entry UI
    public void OnSaveClick()
    {
        var anchor = new AnchorData(anchorID.text, notes.text);
        AnchorUtils.SaveAnchor(anchor);
        onSave?.Invoke(anchor);
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
        anchorID.text = id;
    }

    private void ClearSaveUI()
    {
        anchorID.text = "";
        notes.text = "";
        gameObject.SetActive(false);
    }

}
