using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiMangers : MonoBehaviour
{
    public GameObject uiobjects;

    public void showUI()
    {
        uiobjects.SetActive(true);
    }

    public void hideUI()
    {
        uiobjects.SetActive(false);
    }

    public void ShowandHideUI()
    {
        uiobjects.SetActive(!uiobjects.activeSelf);
    }
}
