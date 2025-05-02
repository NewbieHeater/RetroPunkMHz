using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    public GameObject ChannelUIObject;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChannelUIObject.SetActive(!ChannelUIObject.activeSelf);
        }
    }
}
