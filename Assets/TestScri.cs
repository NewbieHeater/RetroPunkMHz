using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScri : MonoBehaviour
{
    public CinemachineEventAsset CameraAndDialogue;
    public CinemachineEventAsset CameraOnly;
    public CinemachineEventAsset DialogueOnly;
    public CutsceneAsset cutscene;

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


        if (Input.GetKeyDown(KeyCode.S))
        {
            if (cutscene != null)
                CutsceneManager.Instance.Play(cutscene, onFinished: () =>
                {
                    Debug.Log("ÄÆ¾À ³¡!");
                    // ÇÊ¿ä ½Ã DialogueManager.Instance.ResumeDialogue();
                });
        }
    }
}
