using UnityEngine;

public class ReverbObject : MonoBehaviour
{
    public ReverbData reverbData;

    public void Interact()
    {
        Debug.Log("상호작용");


        ReverbUIManager.Instance.currentReverbData = reverbData;
        ReverbUIManager.Instance.ShowAndHideUI();
    }
}
