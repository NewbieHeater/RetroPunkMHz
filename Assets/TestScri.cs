using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScri : MonoBehaviour
{
    public CinemachineEventAsset CameraAndDialogue;
    public CinemachineEventAsset CameraOnly;
    public CinemachineEventAsset DialogueOnly;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            CinemachineEventReader.Instance.PlayEvent(CameraAndDialogue);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            CinemachineEventReader.Instance.PlayEvent(CameraOnly);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            CinemachineEventReader.Instance.PlayEvent(DialogueOnly);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            CinemachineEventReader.Instance.ResetToBaseCam();
        }
    }
}
