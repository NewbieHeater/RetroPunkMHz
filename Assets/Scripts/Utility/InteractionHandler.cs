using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class InteractionHandler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform player; // 플레이어 Transform (없으면 이 컴포넌트가 붙은 오브젝트의 transform 사용)
    [SerializeField] private float scanInterval = 0.05f;
    [SerializeField] private float maxSeekRadius = 5f; // 너무 먼 NPC는 탐색 제외(최대 상호반경보다 크게 잡아도 OK)

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private InteractableBase _focused;
    private float _scanTimer;

    private void Awake()
    {
        if (player == null) player = transform;
    }

    private void Update()
    {
        // 시네머신 이벤트 중엔 상호작용 비활성(원한다면 유지 가능)
        if (CinemachineEventReader.Instance != null && CinemachineEventReader.Instance.IsRunning)
        {
            ClearFocus();
            return;
        }

        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = scanInterval;
            ScanAndFocusNearest();
        }
        
        // PressToInteract 모드: F 키 다운에만 반응
        if (_focused != null && _focused.IsAvailable() && _focused.InRange())
        {
            if (_focused.mode == InteractionMode.PressToInteract)
            {

                var frame = GlobalInputRouter.Instance?.CurrentFrame ?? default;

                if (frame.buttons.IsDown(InputAction.Interact))
                {
                    _focused.TryInteract();
                }
            }
            // AutoOnEnter는 포커스 진입 시 TryInteract()가 이미 호출됨
        }
    }

    private void ScanAndFocusNearest()
    {
        InteractableBase best = null;
        float bestDistSq = float.PositiveInfinity;

        foreach (var it in InteractableBase.All)
        {
            it.BindPlayer(player);

            // 탐색 최대 반경 컷(옵션)
            float rMax = Mathf.Max(it.interactRadius, maxSeekRadius);
            float distSq = it.SqrDistanceToPlayer();
            if (distSq > rMax * rMax) continue; // 너무 멀면 스킵

            // 실제 상호 가능한 거리 내인지 우선순위 ↑
            // (동일 거리면 아무나)
            if (distSq < bestDistSq)
            {
                best = it;
                bestDistSq = distSq;
            }
        }

        if (best != _focused)
        {
            if (_focused != null) _focused.SetFocused(false);
            _focused = best;
            if (_focused != null)
            {
                _focused.SetFocused(_focused.InRange());
            }
        }
        else
        {
            // 같은 대상에 머무를 때도, 거리 변화를 반영해 프롬프트 On/Off 갱신
            if (_focused != null) _focused.SetFocused(_focused.InRange());
        }

        if (showDebug)
        {
            if (_focused != null)
                Debug.Log($"[PlayerInteractor] Focus: {_focused.name} (d={Mathf.Sqrt(bestDistSq):0.00})");
            else
                Debug.Log($"[PlayerInteractor] Focus: (none)");
        }
    }

    private void ClearFocus()
    {
        if (_focused != null) _focused.SetFocused(false);
        _focused = null;
    }
}
