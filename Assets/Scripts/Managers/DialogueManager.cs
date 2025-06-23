using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager Instance
    {
        get { return instance; }
    }

    public DialogueLoader dialogueLoader;  // DialogueLoader�� JSON ������ �Ľ��� ���� �׷��� ������
    public DialogueUI dialogueUI;          // ��ȭ �ؽ�Ʈ, ������, �ʻ�ȭ �� UI�� �����ϴ� ��ũ��Ʈ

    private bool isAuto = false;
    private bool isWaitingForChoice = false;
    private bool isDialogueActive = false;
    private bool isWaitingForEvent = false;
    private string eventResumeNodeId = null;

    // ���� ���� ���� ��ȭ �׷� (�� �׷� �������� id�� "1", "2", ... �� ����)
    private DialogueGroup currentDialogue;
    // ���� �׷� �� ��� ������ id�� ������ ã�� ���� Dictionary
    private Dictionary<string, DialogueLine> currentLineMap = new Dictionary<string, DialogueLine>();
    private DialogueLine currentLine;

    private Coroutine autoAdvanceCoroutine;

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProceedToNext();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // fileName: JSON ���� ���ҽ� ��� (Ȯ���� ����), groupName: "Quest1", "Quest2", "Normal" ��
    public void StartDialogue(string fileName, string groupName)
    {
        dialogueUI.InitCharacters();
        // JSON ������ �ε��ϰ�, ������ �׷��� ��ȭ �����͸� ������
        if (dialogueLoader.LoadDialogueData(fileName))
        {
            currentDialogue = dialogueLoader.GetDialogueGroup(groupName);
            if (currentDialogue == null)
            {
                Debug.LogError("��ȭ �׷��� ã�� �� �����ϴ�: " + groupName);
                return;
            }
            // ���� �׷��� ��� ��� ������ id�� �����ϴ� Dictionary�� ����
            BuildCurrentLineMap();
            isDialogueActive = true;
            dialogueUI.ShowDialoguePanel(true);

            // �Ϲ������� ������ id "1"�� ������ �����Ѵٰ� ����
            if (currentLineMap.TryGetValue("1", out DialogueLine firstLine))
            {
                DisplayDialogueNode(firstLine);
            }
            else
            {
                Debug.LogError("���� ��� (id: \"1\")�� �������� �ʽ��ϴ�.");
                EndDialogue();
            }
        }
    }

    // ���� ��ȭ �׷�(currentDialogue.lines)�� �������� id -> DialogueLine���� ������
    private void BuildCurrentLineMap()
    {
        currentLineMap.Clear();
        if (currentDialogue.lines != null)
        {
            foreach (var line in currentDialogue.lines)
            {
                if (!string.IsNullOrEmpty(line.id))
                {
                    currentLineMap[line.id] = line;
                }
            }
        }
    }

    public void OnLineFinishDisplaying()
    {
        if (!isDialogueActive) return;
        // Auto ����̸� 1�� �� �ڵ����� ���� ���� ����
        if (isAuto && !isWaitingForChoice && !isWaitingForEvent)
        {
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
        }
    }

    private IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (isAuto && !dialogueUI.IsTyping() && !isWaitingForChoice && !isWaitingForEvent)
        {
            ProceedToNext();
        }
        autoAdvanceCoroutine = null;
    }

    public void ProceedToNext()
    {
        if (!isDialogueActive || isWaitingForChoice || isWaitingForEvent) return;

        // ���� Ÿ���� ȿ���� ���� ���̸� ���� �ؽ�Ʈ�� ��� ���
        if (dialogueUI.IsTyping())
        {
            dialogueUI.CompleteTyping();
            return;
        }
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        // ���� ����� �ؽ�Ʈ�� �Բ� �̺�Ʈ Ű�� �����Ǿ� �ִٸ� �̺�Ʈ�� ó��
        if (currentLine != null && !string.IsNullOrEmpty(currentLine.eventKey) && !string.IsNullOrEmpty(currentLine.text))
        {
            bool wait = HandleEvent(currentLine.eventKey);
            if (wait)
            {
                eventResumeNodeId = currentLine.next;
                isWaitingForEvent = true;
                return;
            }
        }

        // ���� ��� id �� currentLine.next�� ���� ("end" �Ǵ� �� ���ڿ��̸� ��ȭ ����)
        string nextId = currentLine != null ? currentLine.next : null;
        if (string.IsNullOrEmpty(nextId) || nextId.ToLower() == "end")
        {
            EndDialogue();
        }
        else
        {
            if (currentLineMap.TryGetValue(nextId, out DialogueLine nextLine))
            {
                DisplayDialogueNode(nextLine);
            }
            else
            {
                Debug.LogError($"���� ��� ID '{nextId}'�� ã�� �� �����ϴ�.");
                EndDialogue();
            }
        }
    }

    // ��ȭ ��带 ȭ�鿡 ǥ���ϰ�, ���� �б�, �̺�Ʈ, ������ ���� ó��
    private void DisplayDialogueNode(DialogueLine line)
    {
        currentLine = line;

        // ���� �̺�Ʈ ���� ����� (�ؽ�Ʈ ���� �̺�Ʈ Ű�� �ִ� ���)
        if (!string.IsNullOrEmpty(line.eventKey) && string.IsNullOrEmpty(line.text))
        {
            bool wait = HandleEvent(line.eventKey);
            string next = line.next;
            if (wait)
            {
                eventResumeNodeId = next;
                isWaitingForEvent = true;
                return;
            }
            if (string.IsNullOrEmpty(next) || next.ToLower() == "end")
            {
                EndDialogue();
            }
            else if (currentLineMap.TryGetValue(next, out DialogueLine nextLine))
            {
                DisplayDialogueNode(nextLine);
            }
            else
            {
                Debug.LogError($"���� ��� ID '{next}'�� ã�� �� �����ϴ�.");
                EndDialogue();
            }
            return;
        }

        // ��簡 �ִ� ��� - UI�� ȭ��, �ؽ�Ʈ, ǥ��(���⼭�� sprite �ʵ� ���) ���� ǥ��
        if (!string.IsNullOrEmpty(line.text))
        {
            // ȭ�� ������ ������ ���� ���� (��: profileMap)��� �����մϴ�.
            CharacterProfile speakerProfile = null;
            speakerProfile = CharacterProfileManager.Instance.GetProfile(line.speaker);
            dialogueUI.ShowDialogueLine(speakerProfile, line.text, line.sprite);
        }

        // �������� �ִ� ��� - UI�� ������ ��ư���� �����Ͽ� ǥ��
        if (line.choices != null && line.choices.Length > 0 && !string.IsNullOrEmpty(line.choices[0].choiceText))
        {
            dialogueUI.ShowChoices(line.choices);
            isWaitingForChoice = true;
        }
        else
        {
            isWaitingForChoice = false;
            // �������� ������ Auto ��� ���� DialogueUI.OnLineFinishDisplaying()���� ó���˴ϴ�.
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (currentLine == null || currentLine.choices == null || choiceIndex < 0 || choiceIndex >= currentLine.choices.Length)
        {
            Debug.LogError("��ȿ���� ���� ������ �ε����Դϴ�.");
            return;
        }

        // ���õ� �������� ������
        Choice selectedChoice = currentLine.choices[choiceIndex];

        dialogueUI.ClearChoices();
        isWaitingForChoice = false;

        // ���⼭ ������ �̺�Ʈ ó��(�ִ� ���)�� �� �� �ֽ��ϴ�.
        // ��: foreach(EventInfo evt in selectedChoice.events) { ProcessEvent(evt); }

        // �������� ���� ���� ��� ��� ID�� �������� �̵�
        string nextId = selectedChoice.next;
        if (string.IsNullOrEmpty(nextId) || nextId.ToLower() == "end")
        {
            EndDialogue();
            return;
        }
        else if (currentLineMap.TryGetValue(nextId, out DialogueLine nextLine))
        {
            // �������� ���� ���� ���� ����
            DisplayDialogueNode(nextLine);
        }
        else
        {
            Debug.LogError($"���� ��� ID '{nextId}'�� ã�� �� �����ϴ�.");
            EndDialogue();
        }
    }


    private void EndDialogue()
    {
        Debug.Log("��ȭ ����");
        isDialogueActive = false;
        dialogueUI.ShowDialoguePanel(false);
        // ���� �߰� ���� �۾� (��: �÷��̾� ���� ���� ��)
    }

    private void OpenShopUI()
    {
        Debug.Log("���� UI ���� - �÷��̾� ���� �̿� ����");
        dialogueUI.ShowDialoguePanel(false);
        // ���� ���� UI ������ ����
    }

    // �̺�Ʈ ó��: OpenShop, Cutscene, Quest �� �̺�Ʈ Ű�� ���� ó��
    private bool HandleEvent(string eventKey)
    {
        Debug.Log($"�̺�Ʈ �߻�: {eventKey}");
        if (eventKey == "OpenShop")
        {
            dialogueUI.ShowDialoguePanel(false);
            OpenShopUI();
            return true;
        }
        else if (eventKey.StartsWith("Cutscene"))
        {
            dialogueUI.ShowDialoguePanel(false);
            // ��: CutsceneManager.Play(eventKey, OnCutsceneFinished);
            return true;
        }
        else if (eventKey.StartsWith("Quest"))
        {
            // ��: QuestManager.TriggerEvent(eventKey);
            return false;
        }
        return false;
    }

    // ������ ���ǽ� �� (�ʿ信 ���� Ȯ��)
    private bool EvaluateCondition(string condition)
    {
        // ��: "playerLevel >= 5" ���� ���ϴ� ���� ����
        return true;
    }

    // �ܺ� �̺�Ʈ(�ƽ�, ���� ���� ��) �Ϸ� �� ��ȭ �簳�� ���� ȣ��
    public void ResumeDialogue()
    {
        if (!isDialogueActive || string.IsNullOrEmpty(eventResumeNodeId)) return;
        dialogueUI.ShowDialoguePanel(true);
        isWaitingForEvent = false;
        if (eventResumeNodeId.ToLower() == "end")
        {
            EndDialogue();
        }
        else if (currentLineMap.TryGetValue(eventResumeNodeId, out DialogueLine nextLine))
        {
            eventResumeNodeId = null;
            DisplayDialogueNode(nextLine);
        }
        else
        {
            Debug.LogError($"�簳�� ��� ID '{eventResumeNodeId}'�� ã�� �� �����ϴ�.");
            EndDialogue();
        }
    }
}
