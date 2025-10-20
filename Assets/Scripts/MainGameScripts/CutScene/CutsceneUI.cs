using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CutsceneUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup _panel;

    [Header("Visuals")]
    [SerializeField] private Image _background;       // 단색 배경
    [SerializeField] private Image _centerImage;      // 중앙 이미지

    [Header("Root")]
    [SerializeField] private Vector2 _baseSize;

    [Header("Explanation")]
    [SerializeField] private TextMeshProUGUI _explanationText;
    [SerializeField] private float _defaultTypingDelay = 0.02f;

    private Coroutine typingCo;
    private string fullText;

    public void Show(bool show)
    {
        if (!_panel) return;
        _panel.alpha = show ? 1f : 0f;
        _panel.blocksRaycasts = show;
        _panel.interactable = show;
    }

    public void SetBackground(Color c)
    {
        if (_background) _background.color = c;
    }

    public void SetCenter(Sprite s, Vector2 size)
    {
        if (!_centerImage) return;
        _centerImage.sprite = s;
        _centerImage.gameObject.SetActive(s != null);
        if (s != null)
        {
            var rt = _centerImage.rectTransform;
            if (size.Equals(Vector2.zero))
            {
                rt.sizeDelta = _baseSize;
                _centerImage.preserveAspect = true;
                return;
            }
            rt.sizeDelta = size;
            _centerImage.preserveAspect = true;
        }
    }

    public void ClearExplanation()
    {
        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = null;
        if (_explanationText) _explanationText.text = "";
    }

    public void ShowExplanation(string text, bool useTyping, float typingDelay = -1f)
    {
        if (!_explanationText) return;
        if (typingCo != null) StopCoroutine(typingCo);

        fullText = text ?? "";

        if (!useTyping)
        {
            _explanationText.text = fullText;
            typingCo = null;
            return;
        }

        float d = typingDelay > 0f ? typingDelay : _defaultTypingDelay;
        typingCo = StartCoroutine(TypeText(fullText, d));
    }

    private IEnumerator TypeText(string content, float delay)
    {
        _explanationText.text = "";
        for (int i = 0; i < content.Length; i++)
        {
            _explanationText.text = content.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(delay);
        }
        typingCo = null;
    }

    public bool IsTyping() => typingCo != null;

    public void CompleteTyping()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }
        if (_explanationText) _explanationText.text = fullText;
    }
}
