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

    public void ShowUserRoleSelection()
    {

    }

    public void OnRoleSelected(UserRole role)
    {
        GameManager.Instance.OnRoleSelected(role);
    }
}
