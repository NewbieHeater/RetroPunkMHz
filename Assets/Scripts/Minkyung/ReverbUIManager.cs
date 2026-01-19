using UnityEngine;
using UnityEngine.UI;

public class ReverbUIManager : MonoBehaviour
{
    public static ReverbUIManager Instance;

    public GameObject reverbPanel;
    public Button playButton;
    public Button cancelButton;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowReverbOptions(System.Action onPlay, System.Action onCancel)
    {
        reverbPanel.SetActive(true);

        playButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        playButton.onClick.AddListener(() =>
        {
            reverbPanel.SetActive(false);
            onPlay?.Invoke();
        });

        cancelButton.onClick.AddListener(() =>
        {
            reverbPanel.SetActive(false);
            onCancel?.Invoke();
        });
    }
}