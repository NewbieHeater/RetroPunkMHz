using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    // Unity 에디터에서 할당해야 하는 UI 참조들
    public GameObject dialoguePanel;   // 대화 전체 패널 (배경 패널 등)
    public TextMeshProUGUI leftNameText;              // 화자 이름 표시 Text
    public TextMeshProUGUI dialogueText;          // 대사 내용 표시 Text (타이핑 대상)
    public Image leftPortrait;         // 좌측 캐릭터 초상화 이미지
    public Image rightPortrait;        // 우측 캐릭터 초상화 이미지
    public Transform choiceContainer;  // 선택지 버튼들을 담는 컨테이너 (예: Vertical Layout Group)
    public Button choiceButtonPrefab;  // 선택지 버튼 프리팹 (미리 설정해둔 UI Button)

    private CharacterProfile leftProfile;
    private CharacterProfile rightProfile;
    private bool isTypingText = false;      // 현재 타이핑 효과 진행 중인지 여부
    private Coroutine typingCoroutine;      // 진행 중인 타이핑 코루틴
    private string currentTypedContent;     // 현재 타이핑 효과로 출력할 전체 문자열

    private void Start()
    {
        //InitCharacters();
    }

    // 대화 패널 활성/비활성화
    public void ShowDialoguePanel(bool show)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(show);
            GameManager.Instance.player.SetAblePlayer(!show);
        }
    }

    // 대화 시작 시 호출: 좌/우 캐릭터 초상화 영역을 초기화
    public void InitCharacters()
    {

        leftPortrait.sprite = CharacterProfileManager.Instance.GetProfile("Eto").GetSprite("netural");
        leftPortrait.color = Color.gray; // 초기엔 비활성 (화자가 나올 때 업데이트)
        leftPortrait.gameObject.SetActive(true);

        rightPortrait.sprite = null;
        rightPortrait.gameObject.SetActive(rightProfile != null);
        if (rightProfile != null)
            rightPortrait.color = Color.gray;
        rightPortrait.gameObject.SetActive(true);
        // 이름 텍스트 초기화
        if (leftNameText != null)
            leftNameText.text = "";
        // 대사 텍스트 초기화
        if (dialogueText != null)
            dialogueText.text = "";
    }

    // 한 줄의 대사를 표시 (화자, 내용, 표정 키 전달)
    public void ShowDialogueLine(CharacterProfile speakerProfile, string content, string expressionKey)
    {
        // 저장: 타이핑 효과에 사용할 전체 텍스트
        currentTypedContent = content;

        // 화자에 따라 이름과 초상화 하이라이트 처리
        if (speakerProfile != null)
        {
            // 이름 표시
            if (leftNameText != null)
                leftNameText.text = speakerProfile.displayName;
            // 화자가 왼쪽 프로필일 경우
            if (speakerProfile.id == "Eto")
            {
                leftPortrait.gameObject.SetActive(true);
                leftPortrait.sprite = speakerProfile.GetSprite(expressionKey);
                leftPortrait.color = Color.white;   // 활성화된 화자는 컬러 표시
                //leftPortrait.SetNativeSize();
                if (rightPortrait != null && rightPortrait.sprite != null)
                {
                    rightPortrait.color = Color.gray;   // 상대방은 회색 처리
                }
            }
            // 화자가 오른쪽 프로필일 경우
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
            // 화자 정보가 없으면 이름 비움 및 양쪽 회색 처리
            if (leftNameText != null)
                leftNameText.text = "";
            if (leftPortrait != null)
                leftPortrait.color = Color.gray;
            if (rightPortrait != null)
                rightPortrait.color = Color.gray;
        }

        // 기존 타이핑 코루틴이 있다면 정지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        // 대사 텍스트 초기화 후 타이핑 효과 시작
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        typingCoroutine = StartCoroutine(TypeText(currentTypedContent));
    }

    // 타이핑 효과 코루틴: 문자열을 한 글자씩 출력
    private IEnumerator TypeText(string content)
    {
        isTypingText = true;
        float delay = 0.03f;  // 글자 출력 딜레이 (필요에 따라 조정)
        for (int i = 0; i < content.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text = content.Substring(0, i + 1);
            yield return new WaitForSeconds(delay);
        }
        isTypingText = false;
        typingCoroutine = null;
        // 대사가 완전히 출력되었음을 DialogueManager에 알림 (Auto 모드 등 처리)
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnLineFinishDisplaying();
    }

    // 현재 타이핑 중인지 여부 반환
    public bool IsTyping()
    {
        return isTypingText;
    }

    // 타이핑 효과 즉시 완료 (남은 텍스트를 한번에 표시)
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

    // 선택지 버튼들을 생성하여 표시 (매개변수 타입을 Choice[]로 변경)
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
                // 선택지 클릭 시 DialogueManager의 SelectChoice 메서드 호출 (구현에 맞춰 수정)
                DialogueManager.Instance.SelectChoice(index);
            });
        }
        if (choiceContainer != null)
            choiceContainer.gameObject.SetActive(true);
    }

    // 모든 선택지 버튼 제거 및 컨테이너 비활성화
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
