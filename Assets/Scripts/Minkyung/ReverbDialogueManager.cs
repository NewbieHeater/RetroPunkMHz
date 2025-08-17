using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReverbDialogueManager : MonoBehaviour
{
    public static ReverbDialogueManager Instance;

    [SerializeField] private GameObject dialoguePanel;        
    [SerializeField] private TextMeshProUGUI speakerText;        
    [SerializeField] private TextMeshProUGUI dialogueText;      
    [SerializeField] private Button nextButton;                   

    private ReverbLine[] currentLines;
    private int currentIndex = 0;

    private void Awake()
    {
        Instance = this;

        dialoguePanel.SetActive(false);

        nextButton.onClick.AddListener(OnNextClicked);
    }

    public void PlayDialogue(ReverbLine[] lines)
    {
        currentLines = lines;
        currentIndex = 0;
        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentIndex < currentLines.Length)
        {
            speakerText.text = currentLines[currentIndex].speaker;
            dialogueText.text = currentLines[currentIndex].text;
        }
        else
        {
            EndDialogue();
        }
    }

    private void OnNextClicked()
    {
        currentIndex++;
        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        speakerText.text = "";
        dialogueText.text = "";
        currentLines = null;
        currentIndex = 0;
    }
}
