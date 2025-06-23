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

    public DialogueLoader dialogueLoader;  // DialogueLoader는 JSON 파일을 파싱해 여러 그룹을 관리함
    public DialogueUI dialogueUI;          // 대화 텍스트, 선택지, 초상화 등 UI를 제어하는 스크립트

    private bool isAuto = false;
    private bool isWaitingForChoice = false;
    private bool isDialogueActive = false;
    private bool isWaitingForEvent = false;
    private string eventResumeNodeId = null;

    // 현재 진행 중인 대화 그룹 (한 그룹 내에서는 id가 "1", "2", ... 로 사용됨)
    private DialogueGroup currentDialogue;
    // 현재 그룹 내 대사 노드들을 id로 빠르게 찾기 위한 Dictionary
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

    // fileName: JSON 파일 리소스 경로 (확장자 없이), groupName: "Quest1", "Quest2", "Normal" 등
    public void StartDialogue(string fileName, string groupName)
    {
        dialogueUI.InitCharacters();
        // JSON 파일을 로드하고, 지정한 그룹의 대화 데이터를 가져옴
        if (dialogueLoader.LoadDialogueData(fileName))
        {
            currentDialogue = dialogueLoader.GetDialogueGroup(groupName);
            if (currentDialogue == null)
            {
                Debug.LogError("대화 그룹을 찾을 수 없습니다: " + groupName);
                return;
            }
            // 현재 그룹의 모든 대사 노드들을 id로 매핑하는 Dictionary를 구성
            BuildCurrentLineMap();
            isDialogueActive = true;
            dialogueUI.ShowDialoguePanel(true);

            // 일반적으로 시작은 id "1"인 대사부터 진행한다고 가정
            if (currentLineMap.TryGetValue("1", out DialogueLine firstLine))
            {
                DisplayDialogueNode(firstLine);
            }
            else
            {
                Debug.LogError("시작 대사 (id: \"1\")가 존재하지 않습니다.");
                EndDialogue();
            }
        }
    }

    // 현재 대화 그룹(currentDialogue.lines)을 기준으로 id -> DialogueLine으로 매핑함
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
        // Auto 모드이면 1초 후 자동으로 다음 대사로 진행
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

        // 만약 타이핑 효과가 진행 중이면 남은 텍스트를 즉시 출력
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

        // 현재 대사의 텍스트와 함께 이벤트 키가 설정되어 있다면 이벤트를 처리
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

        // 다음 노드 id 를 currentLine.next로 참조 ("end" 또는 빈 문자열이면 대화 종료)
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
                Debug.LogError($"다음 대사 ID '{nextId}'를 찾을 수 없습니다.");
                EndDialogue();
            }
        }
    }

    // 대화 노드를 화면에 표시하고, 조건 분기, 이벤트, 선택지 등을 처리
    private void DisplayDialogueNode(DialogueLine line)
    {
        currentLine = line;

        // 만약 이벤트 전용 노드라면 (텍스트 없이 이벤트 키만 있는 경우)
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
                Debug.LogError($"다음 대사 ID '{next}'를 찾을 수 없습니다.");
                EndDialogue();
            }
            return;
        }

        // 대사가 있는 경우 - UI에 화자, 텍스트, 표정(여기서는 sprite 필드 사용) 등을 표시
        if (!string.IsNullOrEmpty(line.text))
        {
            // 화자 프로필 매핑은 별도 관리 (예: profileMap)라고 가정합니다.
            CharacterProfile speakerProfile = null;
            speakerProfile = CharacterProfileManager.Instance.GetProfile(line.speaker);
            dialogueUI.ShowDialogueLine(speakerProfile, line.text, line.sprite);
        }

        // 선택지가 있는 경우 - UI에 선택지 버튼들을 생성하여 표시
        if (line.choices != null && line.choices.Length > 0 && !string.IsNullOrEmpty(line.choices[0].choiceText))
        {
            dialogueUI.ShowChoices(line.choices);
            isWaitingForChoice = true;
        }
        else
        {
            isWaitingForChoice = false;
            // 선택지가 없으면 Auto 모드 등은 DialogueUI.OnLineFinishDisplaying()에서 처리됩니다.
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (currentLine == null || currentLine.choices == null || choiceIndex < 0 || choiceIndex >= currentLine.choices.Length)
        {
            Debug.LogError("유효하지 않은 선택지 인덱스입니다.");
            return;
        }

        // 선택된 선택지를 가져옴
        Choice selectedChoice = currentLine.choices[choiceIndex];

        dialogueUI.ClearChoices();
        isWaitingForChoice = false;

        // 여기서 선택지 이벤트 처리(있는 경우)도 할 수 있습니다.
        // 예: foreach(EventInfo evt in selectedChoice.events) { ProcessEvent(evt); }

        // 선택지에 따른 다음 대사 노드 ID를 가져오고 이동
        string nextId = selectedChoice.next;
        if (string.IsNullOrEmpty(nextId) || nextId.ToLower() == "end")
        {
            EndDialogue();
            return;
        }
        else if (currentLineMap.TryGetValue(nextId, out DialogueLine nextLine))
        {
            // 선택지로 인해 다음 대사로 진행
            DisplayDialogueNode(nextLine);
        }
        else
        {
            Debug.LogError($"다음 대사 ID '{nextId}'를 찾을 수 없습니다.");
            EndDialogue();
        }
    }


    private void EndDialogue()
    {
        Debug.Log("대화 종료");
        isDialogueActive = false;
        dialogueUI.ShowDialoguePanel(false);
        // 이후 추가 정리 작업 (예: 플레이어 제어 복원 등)
    }

    private void OpenShopUI()
    {
        Debug.Log("상점 UI 열림 - 플레이어 상점 이용 가능");
        dialogueUI.ShowDialoguePanel(false);
        // 실제 상점 UI 로직과 연결
    }

    // 이벤트 처리: OpenShop, Cutscene, Quest 등 이벤트 키에 따라 처리
    private bool HandleEvent(string eventKey)
    {
        Debug.Log($"이벤트 발생: {eventKey}");
        if (eventKey == "OpenShop")
        {
            dialogueUI.ShowDialoguePanel(false);
            OpenShopUI();
            return true;
        }
        else if (eventKey.StartsWith("Cutscene"))
        {
            dialogueUI.ShowDialoguePanel(false);
            // 예: CutsceneManager.Play(eventKey, OnCutsceneFinished);
            return true;
        }
        else if (eventKey.StartsWith("Quest"))
        {
            // 예: QuestManager.TriggerEvent(eventKey);
            return false;
        }
        return false;
    }

    // 간단한 조건식 평가 (필요에 따라 확장)
    private bool EvaluateCondition(string condition)
    {
        // 예: "playerLevel >= 5" 등을 평가하는 로직 구현
        return true;
    }

    // 외부 이벤트(컷신, 상점 종료 등) 완료 후 대화 재개를 위해 호출
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
            Debug.LogError($"재개할 대사 ID '{eventResumeNodeId}'를 찾을 수 없습니다.");
            EndDialogue();
        }
    }
}
