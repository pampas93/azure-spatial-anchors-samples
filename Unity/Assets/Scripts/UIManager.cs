// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

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

    [SerializeField] private GameObject UserSelection;
    [SerializeField] private TMP_Text debugText;

    public void ShowUserRoleSelection()
    {
        UserSelection.SetActive(true);
    }

    public void OnRoleSelected(int role)
    {
        GameManager.Instance.OnRoleSelected((UserRole)role);
        UserSelection.SetActive(false);
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
}
