// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
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
    [SerializeField] private SavePinUI saveUI;
    [SerializeField] private LoadPinUI loadPinUI;
    

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

    public void ShowSaveAnchorUI(string id, Action<AnchorData> onSave, Action onCancel)
    {
        saveUI.ShowSaveAnchorUI(id, onSave, onCancel);
    }

    public void LoadAnchorData(AnchorData data)
    {
        loadPinUI.LoadAnchorUI(data);
    }

    public void DeleteAnchorFile()
    {
        AnchorUtils.DeleteAnchorsFile();
    }

    public void CloseApp()
    {
        GameManager.Instance.CloseApp();
    }
}
