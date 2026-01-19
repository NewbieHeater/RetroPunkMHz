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
            Debug.Log("��� ��ҵ�");
        });
    }

    void PlayReverb()
    {
        ReverbDialogueManager.Instance.PlayDialogue(reverbData.dialogue);
    }
}