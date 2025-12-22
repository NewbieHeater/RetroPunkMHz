using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : UI_Popup
{
    private enum GameObjects
    {
        DialoguePanel,
        ChoiceContainer,
        LeftPortrait,
        RightPortrait,
    }

    private enum Texts
    {
        NameText,
        DialogueText,
    }

    private enum Buttons
    {
        ChangeAuto,
    }

    [SerializeField]private GameObject _dialoguePanel;
    [SerializeField]private Transform _choiceContainer;
    [SerializeField]private Image _leftPortrait;
    [SerializeField]private Image _rightPortrait;

    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _dialogueText;

    private Button _changeAutoButton;

    [SerializeField] private float delay = 0.03f;
    [SerializeField] private DialogueManager _dialogueManager;

    private readonly List<UI_Dialogue_ChoiceSlot> _choiceSlots = new List<UI_Dialogue_ChoiceSlot>();

    private bool _isTypingText;
    private Coroutine _typingCoroutine;
    private string _currentTypedContent;

    private void Awake()
    {
        Init();
    }

    public void SetManager(DialogueManager dialogueManager)
    {
        _dialogueManager = dialogueManager;
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        _dialoguePanel = Get<GameObject>((int)GameObjects.DialoguePanel);
        _choiceContainer = Get<GameObject>((int)GameObjects.ChoiceContainer).transform;

        _leftPortrait = Get<GameObject>((int)GameObjects.LeftPortrait).GetComponent<Image>();
        _rightPortrait = Get<GameObject>((int)GameObjects.RightPortrait).GetComponent<Image>();

        _nameText = Get<TextMeshProUGUI>((int)Texts.NameText);
        _dialogueText = Get<TextMeshProUGUI>((int)Texts.DialogueText);

        _changeAutoButton = Get<Button>((int)Buttons.ChangeAuto);
        if (_changeAutoButton != null)
        {
            _changeAutoButton.onClick.RemoveAllListeners();
            _changeAutoButton.onClick.AddListener(ToggleAuto);
        }

        // 선택지 컨테이너 초기 비활성
        if (_choiceContainer != null)
            _choiceContainer.gameObject.SetActive(false);

        InitCharacters();
    }

    private void ToggleAuto()
    {
        _dialogueManager?.ToggleAuto();
    }

    public void ShowDialoguePanel(bool show)
    {
        if (_dialoguePanel != null)
        {
            _dialoguePanel.SetActive(show);
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                GameManager.Instance.player.SetAblePlayer(!show);
        }
    }

    public void InitCharacters()
    {
        var eto = Managers.Profile.GetProfile("Eto");

        if (_leftPortrait != null)
        {
            _leftPortrait.sprite = eto?.GetSprite("neutral");
            _leftPortrait.color = Color.gray;
            _leftPortrait.gameObject.SetActive(true);
        }

        if (_rightPortrait != null)
        {
            _rightPortrait.sprite = null;
            _rightPortrait.color = Color.gray;
            _rightPortrait.gameObject.SetActive(true);
        }

        if (_nameText != null) _nameText.text = "";
        if (_dialogueText != null) _dialogueText.text = "";
    }

    public void ShowDialogueLine(CharacterProfile speakerProfile, string content, string expressionKey)
    {
        if (speakerProfile == null)
            return;

        _currentTypedContent = content ?? string.Empty;

        if (_nameText != null)
            _nameText.text = speakerProfile.displayName ?? string.Empty;

        if (speakerProfile.defaultSide == CharacterSide.Left)
        {
            if (_leftPortrait != null)
            {
                _leftPortrait.gameObject.SetActive(true);
                _leftPortrait.sprite = speakerProfile.GetSprite(expressionKey);
                _leftPortrait.color = Color.white;
            }

            if (_rightPortrait != null)
            {
                if (_rightPortrait.sprite == null) _rightPortrait.gameObject.SetActive(false);
                else _rightPortrait.color = Color.gray;
            }
        }
        else
        {
            if (_rightPortrait != null)
            {
                _rightPortrait.gameObject.SetActive(true);
                _rightPortrait.sprite = speakerProfile.GetSprite(expressionKey);
                _rightPortrait.color = Color.white;
            }

            if (_leftPortrait != null)
            {
                if (_leftPortrait.sprite == null) _leftPortrait.gameObject.SetActive(false);
                else _leftPortrait.color = Color.gray;
            }
        }

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        if (_dialogueText != null)
            _dialogueText.text = "";

        _typingCoroutine = StartCoroutine(TypeText(_currentTypedContent));
    }

    private IEnumerator TypeText(string content)
    {
        _isTypingText = true;

        for (int i = 0; i < content.Length; i++)
        {
            if (_dialogueText != null)
                _dialogueText.text = content.Substring(0, i + 1);

            yield return new WaitForSeconds(delay);
        }

        _isTypingText = false;
        _typingCoroutine = null;

        _dialogueManager?.OnLineFinishDisplaying();
    }

    public bool IsTyping() => _isTypingText;

    public void CompleteTyping()
    {
        if (_isTypingText && _typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        if (_dialogueText != null)
            _dialogueText.text = _currentTypedContent ?? string.Empty;

        _isTypingText = false;
        _dialogueManager?.OnLineFinishDisplaying();
    }

    // === UI_Shop 스타일: 슬롯 풀링/재사용 ===
    public void RefreshChoices(Choice[] choices)
    {
        if (_choiceContainer == null)
            return;

        int needed = (choices == null) ? 0 : choices.Length;

        // 1) 슬롯 부족하면 추가 생성
        while (_choiceSlots.Count < needed)
        {
            var slot = Managers.UI.MakeSubItem<UI_Dialogue_ChoiceSlot>(_choiceContainer);
            _choiceSlots.Add(slot);
        }

        // 2) 필요한 만큼 세팅 + 활성화
        for (int i = 0; i < needed; i++)
        {
            var c = choices[i];
            var slot = _choiceSlots[i];

            bool valid = (c != null);

            if (slot != null)
                slot.gameObject.SetActive(valid);

            if (valid)
                slot.Setup(_dialogueManager, i, c.choiceText);
        }

        // 3) 남는 슬롯 비활성화
        for (int i = needed; i < _choiceSlots.Count; i++)
        {
            var slot = _choiceSlots[i];
            if (slot != null && slot.gameObject.activeSelf)
                slot.gameObject.SetActive(false);
        }

        _choiceContainer.gameObject.SetActive(needed > 0);
    }

    public void ClearChoices()
    {
        if (_choiceContainer != null)
            _choiceContainer.gameObject.SetActive(false);

        for (int i = 0; i < _choiceSlots.Count; i++)
        {
            var slot = _choiceSlots[i];
            if (slot != null && slot.gameObject.activeSelf)
                slot.gameObject.SetActive(false);
        }
    }
}
