using UnityEngine;
using TMPro;

public class ReverbDialogueManager : MonoBehaviour
{
    public static ReverbDialogueManager Instance;

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;

        Invoke(nameof(HideDialogue), 5f);
    }

    private void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}