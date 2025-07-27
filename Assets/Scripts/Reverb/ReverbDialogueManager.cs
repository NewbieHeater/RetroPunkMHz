using UnityEngine;
using TMPro;

public class ReverbDialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;

        // 일정 시간 후 자동으로 닫기
        Invoke(nameof(HideDialogue), 5f);
    }

    private void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
