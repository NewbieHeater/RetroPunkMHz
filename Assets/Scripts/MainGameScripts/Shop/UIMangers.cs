using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMangers : MonoBehaviour
{
    public GameObject UIObjects;

    public void ShowUI()
    {
        UIObjects.SetActive(true);
    }

    public void HideUI()
    {
        UIObjects.SetActive(false);
    }

    public void TogleUI()
    {
        UIObjects.SetActive(!UIObjects.activeSelf);
    }
}
