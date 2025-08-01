using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    public UIMangers uiMangers;
    public void Interact()
    {
        Debug.Log("상호작용");

        uiMangers.ShowAndHideUI();

    }


}
