using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopKeeper : MonoBehaviour, IInteractable
{

    public UIManger UIMangers;

    public string GetInteractPrompt()
    {
        return "F키를 눌러라";
    }

    public void Interact()
    { 

        UIManger.Instance.TogleUI();

    }


}
