using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    public GameObject ChannelUIObject;

    private void Start()
    {
        ChannelUIObject.SetActive(false);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ChannelUIObject.SetActive(!ChannelUIObject.activeSelf);
        }
    }
}
