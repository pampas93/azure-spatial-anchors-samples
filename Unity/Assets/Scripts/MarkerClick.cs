// Author: Abhijit Srikanth (abhijit.93@hotmail.com)

using System;
using UnityEngine;

public class MarkerClick : MonoBehaviour
{
    private string anchorIdentifier;
    private Action onClick;

    public void MarkerSetup(string id, Action onClick)
    {
        anchorIdentifier = id;
        this.onClick = onClick;
    }

    public void OnClick()
    {
        onClick?.Invoke();
    }
}
