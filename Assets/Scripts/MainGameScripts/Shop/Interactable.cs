using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{

    public UIMangers uiMangers;

    public string GetInteractPrompt()
    {
        return "F를 눌러 상호작용";
    }

    public void Interact()
    {
        Debug.Log("상호작용");

        uiMangers.TogleUI();

    }


}
