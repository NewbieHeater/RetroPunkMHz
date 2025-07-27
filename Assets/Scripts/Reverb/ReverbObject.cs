using UnityEngine;

public class ReverbObject : MonoBehaviour
{

    public ReverbData reverbData;


    public void Interact()
    {
        ReverbUIManager.Instance.ShowReverbOptions(() =>
        {
            PlayReverb();
        },
        () =>
        {
            Debug.Log("재생 취소됨");
        });
    }

    void PlayReverb()
    {
        ReverbDialogueManager.Instance.PlayDialogue(reverbData.dialogue);
    }
}
