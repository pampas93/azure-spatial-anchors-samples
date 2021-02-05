// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { 
        get {
            return instance;
        } 
    }

    private static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private GameObject userSelection;
    [SerializeField] private TMP_Text debugText;

    [Header("SAve UI properties")]
    [SerializeField] private GameObject saveUI;
    [SerializeField] private TMP_Text anchorID;
    [SerializeField] private TMP_InputField notes;

    Action<AnchorData> onSave;
    Action onCancel;

    public void ShowUserRoleSelection()
    {
        userSelection.SetActive(true);
    }

    public void OnRoleSelected(int role)
    {
        GameManager.Instance.OnRoleSelected((UserRole)role);
        userSelection.SetActive(false);
    }

    public void SetDebugText(string str)
    {
        Debug.Log("SpatialNotes: " + str);
        debugText.text = str;
    }

    public void StartApp()
    {
        GameManager.Instance.SwitchAppMode(AppState.Start);
    }

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
        saveUI.SetActive(true);
        this.onSave = onSave;
        this.onCancel = onCancel;
        anchorID.text = id;
    }

    private void ClearSaveUI()
    {
        anchorID.text = "";
        notes.text = "";
        saveUI.SetActive(false);
    }

    public void DeleteAnchorFile()
    {
        AnchorUtils.DeleteAnchorsFile();
    }
}
