using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private readonly List<IInteractable> _candidates = new();
    private IInteractable _current;

    void Awake() { if (promptUI) promptUI.SetActive(false); }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var it))
        {
            if (!_candidates.Contains(it)) _candidates.Add(it);
            UpdateCurrent();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var it))
        {
            _candidates.Remove(it);
            if (_current == it) _current = null;
            UpdateCurrent();
        }
    }

    void Update()
    {
        if (_current != null && Input.GetKeyDown(interactKey))
        {
            _current.Interact();
            // 상호작용 중에는 프롬프트 숨김(대화 UI가 따로 뜨면 충돌 방지)
            if (promptUI) promptUI.SetActive(false);
        }
    }

    private void UpdateCurrent()
    {
        // 가장 가까운 대상을 선택
        float best = float.MaxValue;
        IInteractable pick = null;
        foreach (var it in _candidates)
        {
            if (it is Component c)
            {
                float d = (c.transform.position - transform.position).sqrMagnitude;
                if (d < best) { best = d; pick = it; }
            }
        }
        _current = pick;

        if (_current != null)
        {
            if (promptText) promptText.text = _current.GetInteractPrompt();
            if (promptUI) promptUI.SetActive(true);
        }
        else
        {
            if (promptUI) promptUI.SetActive(false);
        }
    }
}
