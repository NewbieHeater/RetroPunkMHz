using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    public uiMangers uiMangers;
    public void Interact()
    {
       

        uiMangers.ShowandHideUI();

    }


}
