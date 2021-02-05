using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerClick : MonoBehaviour
{
    public Action OnClick;

    private void OnMouseDown()
    {
        OnClick?.Invoke();
    }
}
