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

        // ���� �ð� �� �ڵ����� �ݱ�
        Invoke(nameof(HideDialogue), 5f);
    }

    private void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
