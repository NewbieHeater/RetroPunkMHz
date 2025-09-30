using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum InteractionMode
{
    PressToInteract, // F를 눌러 상호작용
    AutoOnEnter      // 범위 안에 들어오면 자동 상호작용
}

public abstract class InteractableBase : MonoBehaviour
{
    // === Static Registry ===
    private static readonly HashSet<InteractableBase> _registry = new HashSet<InteractableBase>();
    public static IReadOnlyCollection<InteractableBase> All => _registry;

    protected virtual void OnEnable() => _registry.Add(this);
    protected virtual void OnDisable() => _registry.Remove(this);

    [Header("Interaction")]
    [Min(0f)] public float interactRadius = 2.0f;
    public InteractionMode mode = InteractionMode.PressToInteract;
    [Tooltip("한 번만 동작하도록 할지 여부")] public bool singleUse = false;

    [Header("Prompt (Head-up hint)")]
    [SerializeField] private string pressPrompt = "F: 상호작용";
    [SerializeField] private string autoPrompt = "F를 눌러 상호작용";
    [Header("Prompt UI")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private bool _consumed = false;
    private bool _focused = false; // 현재 플레이어가 '주 대상'으로 보고 있는지
    private Transform _player;

    public void BindPlayer(Transform player) => _player = player;
    public Transform BoundPlayer => _player;

    public float SqrDistanceToPlayer()
    {
        if (_player == null) return float.PositiveInfinity;
        return (transform.position - _player.position).sqrMagnitude;
    }

    public bool InRange()
    {
        float r = interactRadius <= 0f ? 0f : interactRadius;
        return SqrDistanceToPlayer() <= r * r;
    }

    public bool IsAvailable() => !_consumed;

    // === Focus & Prompt ===
    public void SetFocused(bool focused)
    {
        if (_focused == focused) return;
        _focused = focused;

        if (_focused)
        {
            // 포커스 진입: NPC별 안내 표시
            OnShowPrompt(GetPromptText());
            if (mode == InteractionMode.AutoOnEnter)
            {
                // 자동 상호작용 모드면 즉시 시도
                TryInteract();
            }
        }
        else
        {
            OnHidePrompt();
        }
    }

    protected virtual string GetPromptText()
    {
        return !string.IsNullOrEmpty(pressPrompt) ? pressPrompt : autoPrompt;
    }

    // UI 훅: 필요 시 여기서 월드 스페이스 캔버스/아이콘 갱신. 현재는 Log로 대체.
    protected virtual void OnShowPrompt(string text)
    {
        if (promptUI != null) promptUI.SetActive(true);
        promptText.text = text;
    }

    protected virtual void OnHidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    protected virtual void ToglePrompt(string text)
    {
        if (promptUI != null) promptUI.SetActive(!promptUI.activeSelf);
        promptText.text = text;
    }

    public static void ReevaluateAllPrompts()
    {
        foreach (var it in All) it.ReevaluatePrompt();
    }

    public static void HideAllPrompts()
    {
        foreach (var it in All)
        {
            it.OnHidePrompt();
        }
    }

    public void ReevaluatePrompt()
    {
        bool shouldShow = _focused && InRange() && IsAvailable();

        if (shouldShow && !promptUI.activeSelf)
        {
            OnShowPrompt(GetPromptText());
        }
        else if (!shouldShow && promptUI.activeSelf)
        {
            OnHidePrompt();
        }
    }

    // === 실제 상호작용 ===
    public void TryInteract()
    {
        if (!IsAvailable()) return;
        if (!InRange()) return;
        if (OnInteract())
        {
            if (singleUse) _consumed = true;
            // 성공 시 프롬프트는 숨김(상태에 따라 유지하고 싶으면 주석 처리)
            ToglePrompt(GetPromptText());
        }
    }

    /// <summary>
    /// 상속 객체에서 구체 동작 구현
    /// </summary>
    protected virtual bool OnInteract()
    {

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
#endif
}
