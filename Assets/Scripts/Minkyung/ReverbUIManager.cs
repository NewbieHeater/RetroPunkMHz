using UnityEngine;
using UnityEngine.UI;

public class ReverbUIManager : MonoBehaviour
{
    public static ReverbUIManager Instance;

    public GameObject reverbPanel; 
    public Button yesButton;
    public Button noButton;

    public ReverbData currentReverbData; 

    void Awake()
    {
        Instance = this;
        reverbPanel.SetActive(false);
    }

    public void ShowAndHideUI()
    {
        reverbPanel.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            reverbPanel.SetActive(false);
            if (currentReverbData != null)
                ReverbDialogueManager.Instance.PlayDialogue(currentReverbData.lines);
        });

        noButton.onClick.AddListener(() =>
        {
            reverbPanel.SetActive(false);
            Debug.Log("재생 취소됨");
        });
    }
}
