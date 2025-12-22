using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue_ChoiceSlot : UI_Base
{
    private enum Texts
    {
        ChoiceText,
    }

    private Button _button;
    private TextMeshProUGUI _choiceText;

    private DialogueManager _dialogueManager;
    private int _index;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));

        _button = GetComponent<Button>();
        _choiceText = Get<TextMeshProUGUI>((int)Texts.ChoiceText);

        // 중복 리스너 방지
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }
    }

    public void Setup(DialogueManager mgr, int index, string text)
    {
        _dialogueManager = mgr;
        _index = index;

        if (_choiceText != null)
            _choiceText.text = text;
    }

    private void OnClick()
    {
        _dialogueManager?.SelectChoice(_index);
    }
}
