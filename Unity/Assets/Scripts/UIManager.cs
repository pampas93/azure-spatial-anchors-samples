// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void ShowUserRoleSelection()
    {
        UserSelection.SetActive(true);
    }

    public void OnRoleSelected(int role)
    {
        GameManager.Instance.OnRoleSelected((UserRole)role);
        UserSelection.SetActive(false);
    }
}
