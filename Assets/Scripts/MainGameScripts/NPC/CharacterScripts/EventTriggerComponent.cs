using UnityEngine;
using UnityEngine.Events;

public class EventTriggerComponent : MonoBehaviour
{
    public enum FireMode
    {
        ProximityAuto,     // 플레이어가 범위/콜라이더에 들어오면 자동 발화
        ProximityInteract, // 범위 안에서 상호작용 키(Interact) 눌러야 발화
        Manual             // 코드에서 Activate()를 호출해야 발화
    }

    [Header("Trigger Mode")]
    [SerializeField] private FireMode mode = FireMode.ProximityAuto;

    [Header("Proximity Settings")]
    [Tooltip("플레이어를 식별할 태그")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Trigger 콜라이더 대신 반경 검사로도 사용 가능(콜라이더가 없어도 동작)")]
    [SerializeField] private bool useRadiusCheck = false;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private LayerMask playerLayer = ~0;

    [Header("Interaction")]
    [Tooltip("ProximityInteract 모드일 때, Interact 키(예: E) 입력을 요구")]
    [SerializeField] private bool requireLookAt = false;  // 시선 요구가 필요하면 향후 확장
    [SerializeField] private float interactMaxDistance = 2.5f;

    [Header("Lifecycle")]
    [SerializeField] private bool oneShot = true;        // 한 번만 발화
    [SerializeField] private float cooldown = 0f;        // 재발화까지 대기 시간

    [Header("Outputs")]
    public UnityEvent OnTriggered;                       // 일반 유니티 이벤트 훅
    [SerializeField] private CinemachineEventAsset cmEvent; // 있으면 시네머신 이벤트도 실행

    // 내부 상태
    private bool _armed = true;
    private float _coolRemain = 0f;
    private Transform _player;

    private void Awake()
    {
        // 범위체크 전용이면 콜라이더 없이도 OK
        // 콜라이더를 쓴다면 isTrigger=true 권장
    }

    private void Update()
    {
        if (!_armed)
        {
            if (_coolRemain > 0f)
            {
                _coolRemain -= Time.deltaTime;
                if (_coolRemain <= 0f && !oneShot) _armed = true;
            }
            return;
        }

        if (mode == FireMode.Manual) return; // 수동 모드는 Update 검사 안 함

        // 플레이어 캐칭
        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) _player = go.transform;
        }

        if (_player == null) return;

        bool inRange = false;

        if (useRadiusCheck)
        {
            float dist = Vector3.Distance(_player.position, transform.position);
            inRange = dist <= radius;
        }
        // 콜라이더 트리거를 쓰는 경우엔 OnTriggerEnter/Stay로도 처리 가능
        // 여기선 radius OR collider 어느 쪽이든 true면 발화 기회 제공

        if (inRange)
        {
            switch (mode)
            {
                case FireMode.ProximityAuto:
                    TryFire();
                    break;
                case FireMode.ProximityInteract:
                    // 상호작용 키 확인(글로벌 인풋 라우터 이용)
                    var frame = GlobalInputRouter.Instance.CurrentFrame;
                    if (frame.buttons.IsDown(InputAction.Interact))
                    {
                        if (!requireLookAt ||
                            Vector3.Distance(_player.position, transform.position) <= interactMaxDistance)
                        {
                            TryFire();
                        }
                    }
                    break;
            }
        }
    }

    // 콜라이더 트리거 사용 시(옵션)
    private void OnTriggerStay(Collider other)
    {
        if (!_armed || mode == FireMode.Manual || useRadiusCheck) return;
        if (!other.CompareTag(playerTag)) return;

        if (mode == FireMode.ProximityAuto)
        {
            TryFire();
        }
        else if (mode == FireMode.ProximityInteract)
        {
            var frame = GlobalInputRouter.Instance ? GlobalInputRouter.Instance.CurrentFrame : default;
            if (frame.buttons.IsDown(InputAction.Interact))
            {
                TryFire();
            }
        }
    }

    /// <summary>
    /// 외부 스크립트에서 수동 발화
    /// </summary>
    public void Activate()
    {
        if (!_armed) return;
        TryFire();
    }

    private void TryFire()
    {
        if (!_armed) return;


        // 시네머신 이벤트
        if (cmEvent != null)
        {
            // 옵션: 이벤트 에셋의 설정 일부를 런타임에서 오버라이드
            //cmEvent.restoreCameraAfter = restoreCameraAfter;
            CinemachineEventReader.Instance.PlayEvent(cmEvent);
        }

        // UnityEvent 콜백
        OnTriggered?.Invoke();

        // 쿨다운/원샷 처리
        if (oneShot)
        {
            _armed = false;
        }
        else if (cooldown > 0f)
        {
            _armed = false;
            _coolRemain = cooldown;
        }

        // 입력 해제는 이벤트 쪽에서 끝날 때 호출하는게 가장 안전
        // 만약 즉시 해제하고 싶다면 아래 주석 해제
        // if (lockPlayerInputWhileRunning && GlobalInputRouter.Instance)
        //     GlobalInputRouter.Instance.LockInput(false);
    }

    /// <summary> 외부에서 재무장 </summary>
    public void Rearm()
    {
        _coolRemain = 0f;
        _armed = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (useRadiusCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
#endif
}
