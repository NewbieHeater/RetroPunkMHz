using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    // Unity �����Ϳ��� �Ҵ��ؾ� �ϴ� UI ������
    public GameObject dialoguePanel;   // ��ȭ ��ü �г� (��� �г� ��)
    public TextMeshProUGUI leftNameText;              // ȭ�� �̸� ǥ�� Text
    public TextMeshProUGUI dialogueText;          // ��� ���� ǥ�� Text (Ÿ���� ���)
    public Image leftPortrait;         // ���� ĳ���� �ʻ�ȭ �̹���
    public Image rightPortrait;        // ���� ĳ���� �ʻ�ȭ �̹���
    public Transform choiceContainer;  // ������ ��ư���� ��� �����̳� (��: Vertical Layout Group)
    public Button choiceButtonPrefab;  // ������ ��ư ������ (�̸� �����ص� UI Button)

    private CharacterProfile leftProfile;
    private CharacterProfile rightProfile;
    private bool isTypingText = false;      // ���� Ÿ���� ȿ�� ���� ������ ����
    private Coroutine typingCoroutine;      // ���� ���� Ÿ���� �ڷ�ƾ
    private string currentTypedContent;     // ���� Ÿ���� ȿ���� ����� ��ü ���ڿ�

    private void Start()
    {
        //InitCharacters();
    }

    // ��ȭ �г� Ȱ��/��Ȱ��ȭ
    public void ShowDialoguePanel(bool show)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(show);
            GameManager.Instance.player.SetAblePlayer(!show);
        }
    }

    // ��ȭ ���� �� ȣ��: ��/�� ĳ���� �ʻ�ȭ ������ �ʱ�ȭ
    public void InitCharacters()
    {

        leftPortrait.sprite = CharacterProfileManager.Instance.GetProfile("Eto").GetSprite("netural");
        leftPortrait.color = Color.gray; // �ʱ⿣ ��Ȱ�� (ȭ�ڰ� ���� �� ������Ʈ)
        leftPortrait.gameObject.SetActive(true);

        rightPortrait.sprite = null;
        rightPortrait.gameObject.SetActive(rightProfile != null);
        if (rightProfile != null)
            rightPortrait.color = Color.gray;
        rightPortrait.gameObject.SetActive(true);
        // �̸� �ؽ�Ʈ �ʱ�ȭ
        if (leftNameText != null)
            leftNameText.text = "";
        // ��� �ؽ�Ʈ �ʱ�ȭ
        if (dialogueText != null)
            dialogueText.text = "";
    }

    // �� ���� ��縦 ǥ�� (ȭ��, ����, ǥ�� Ű ����)
    public void ShowDialogueLine(CharacterProfile speakerProfile, string content, string expressionKey)
    {
        // ����: Ÿ���� ȿ���� ����� ��ü �ؽ�Ʈ
        currentTypedContent = content;

        // ȭ�ڿ� ���� �̸��� �ʻ�ȭ ���̶���Ʈ ó��
        if (speakerProfile != null)
        {
            // �̸� ǥ��
            if (leftNameText != null)
                leftNameText.text = speakerProfile.displayName;
            // ȭ�ڰ� ���� �������� ���
            if (speakerProfile.id == "Eto")
            {
                leftPortrait.gameObject.SetActive(true);
                leftPortrait.sprite = speakerProfile.GetSprite(expressionKey);
                leftPortrait.color = Color.white;   // Ȱ��ȭ�� ȭ�ڴ� �÷� ǥ��
                //leftPortrait.SetNativeSize();
                if (rightPortrait != null && rightPortrait.sprite != null)
                {
                    rightPortrait.color = Color.gray;   // ������ ȸ�� ó��
                }
            }
            // ȭ�ڰ� ������ �������� ���
            else if (speakerProfile.id != "Eto")
            {
                rightPortrait.gameObject.SetActive(true);
                rightPortrait.sprite = speakerProfile.GetSprite(expressionKey);
                rightPortrait.color = Color.white;
                //rightPortrait.SetNativeSize();
                if (leftPortrait != null && leftPortrait.sprite != null)
                {
                    leftPortrait.color = Color.gray;
                }
            }
            
        }
        else
        {
            // ȭ�� ������ ������ �̸� ��� �� ���� ȸ�� ó��
            if (leftNameText != null)
                leftNameText.text = "";
            if (leftPortrait != null)
                leftPortrait.color = Color.gray;
            if (rightPortrait != null)
                rightPortrait.color = Color.gray;
        }

        // ���� Ÿ���� �ڷ�ƾ�� �ִٸ� ����
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        // ��� �ؽ�Ʈ �ʱ�ȭ �� Ÿ���� ȿ�� ����
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        typingCoroutine = StartCoroutine(TypeText(currentTypedContent));
    }

    // Ÿ���� ȿ�� �ڷ�ƾ: ���ڿ��� �� ���ھ� ���
    private IEnumerator TypeText(string content)
    {
        isTypingText = true;
        float delay = 0.03f;  // ���� ��� ������ (�ʿ信 ���� ����)
        for (int i = 0; i < content.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text = content.Substring(0, i + 1);
            yield return new WaitForSeconds(delay);
        }
        isTypingText = false;
        typingCoroutine = null;
        // ��簡 ������ ��µǾ����� DialogueManager�� �˸� (Auto ��� �� ó��)
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnLineFinishDisplaying();
    }

    // ���� Ÿ���� ������ ���� ��ȯ
    public bool IsTyping()
    {
        return isTypingText;
    }

    // Ÿ���� ȿ�� ��� �Ϸ� (���� �ؽ�Ʈ�� �ѹ��� ǥ��)
    public void CompleteTyping()
    {
        if (isTypingText && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (dialogueText != null && !string.IsNullOrEmpty(currentTypedContent))
            dialogueText.text = currentTypedContent;
        isTypingText = false;
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnLineFinishDisplaying();
    }

    // ������ ��ư���� �����Ͽ� ǥ�� (�Ű����� Ÿ���� Choice[]�� ����)
    public void ShowChoices(Choice[] choices)
    {
        ClearChoices();
        for (int i = 0; i < choices.Length; i++)
        {
            Choice choice = choices[i];
            if (choice == null || string.IsNullOrEmpty(choice.choiceText))
                continue;
            Button choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = choice.choiceText;
            int index = i;
            choiceButton.onClick.AddListener(() => {
                // ������ Ŭ�� �� DialogueManager�� SelectChoice �޼��� ȣ�� (������ ���� ����)
                DialogueManager.Instance.SelectChoice(index);
            });
        }
        if (choiceContainer != null)
            choiceContainer.gameObject.SetActive(true);
    }

    // ��� ������ ��ư ���� �� �����̳� ��Ȱ��ȭ
    public void ClearChoices()
    {
        if (choiceContainer == null)
            return;
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
        choiceContainer.gameObject.SetActive(false);
    }
}
