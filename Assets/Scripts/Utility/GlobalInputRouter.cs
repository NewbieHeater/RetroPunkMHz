// using UnityEditor; // 빌드 대상이면 제거 권장
using UnityEngine;

public enum InputAction
{
    Jump,         // 점프
    SprintToggle, // 스프린트 토글
    Interact,     // 상호작용
    Attack,       // 좌클릭
    Charge        // 우클릭(차지)
}

public struct FrameButtons
{
    private int _downMask;
    private int _heldMask;
    private int _upMask;

    public FrameButtons(int downMask, int heldMask, int upMask)
    {
        _downMask = downMask;
        _heldMask = heldMask;
        _upMask = upMask;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsDown(InputAction a) { int b = 1 << (int)a; return (_downMask & b) != 0; }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsHeld(InputAction a) { int b = 1 << (int)a; return (_heldMask & b) != 0; }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsUp(InputAction a) { int b = 1 << (int)a; return (_upMask & b) != 0; }

    public int RawDownMask => _downMask;
    public int RawHeldMask => _heldMask;
    public int RawUpMask => _upMask;

    // 마스크 적용(허용된 비트만 남김)
    public FrameButtons FilterByAllowMask(int allowMask)
    {
        return new FrameButtons(_downMask & allowMask, _heldMask & allowMask, _upMask & allowMask);
    }
}

public struct PlayerInputFrame
{
    public float moveX;        // 축
    public FrameButtons buttons;

    public PlayerInputFrame(float moveX, FrameButtons buttons)
    {
        this.moveX = moveX;
        this.buttons = buttons;
    }
}

public class GlobalInputRouter : Singleton<GlobalInputRouter>
{
    [Header("Bindings (Legacy Input Manager names)")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";
    public string sprintToggleButton = "SprintToggle";
    public string interactButton = "Interact";
    public string attackButton = "Fire1";  // 좌클릭
    public string chargeButton = "Fire2";  // 우클릭

    [Header("Lock Options")]
    [Tooltip("입력 잠금 시에도 축 입력을 허용할지(기본 false)")]
    [SerializeField] private bool allowAxisWhileLocked = false;

    public bool IsLocked => _locked;
    public PlayerInputFrame CurrentFrame { get; private set; }

    private bool _locked;
    private int _allowMaskWhenLocked; // 잠금 중 통과 허용되는 버튼 비트

    void Update()
    {
        int down = 0, held = 0, up = 0;

        // 버튼 누적
        AccumulateButton(jumpButton, InputAction.Jump, ref down, ref held, ref up);
        AccumulateButton(sprintToggleButton, InputAction.SprintToggle, ref down, ref held, ref up);
        AccumulateButton(interactButton, InputAction.Interact, ref down, ref held, ref up);
        AccumulateButton(attackButton, InputAction.Attack, ref down, ref held, ref up);
        AccumulateButton(chargeButton, InputAction.Charge, ref down, ref held, ref up);

        // 축 입력
        float moveX = 0f;
        if (!string.IsNullOrEmpty(horizontalAxis))
            moveX = Input.GetAxisRaw(horizontalAxis);

        // 프레임 구성
        FrameButtons fb = new FrameButtons(down, held, up);

        if (_locked)
        {
            // 버튼: 허용 마스크만 통과
            fb = fb.FilterByAllowMask(_allowMaskWhenLocked);
            // 축: 필요 시 0 처리
            if (!allowAxisWhileLocked) moveX = 0f;
        }

        CurrentFrame = new PlayerInputFrame(moveX, fb);
    }

    private void AccumulateButton(string buttonName, InputAction action, ref int down, ref int held, ref int up)
    {
        if (string.IsNullOrEmpty(buttonName)) return;
        int bit = 1 << (int)action;

        if (Input.GetButtonDown(buttonName)) down |= bit;
        if (Input.GetButton(buttonName)) held |= bit;
        if (Input.GetButtonUp(buttonName)) up |= bit;
    }

    /// <summary>
    /// 입력 잠금/해제. allowActions가 지정되면 잠금 중 해당 버튼만 통과.
    /// </summary>
    public void LockInput(bool locked, InputAction[] allowActions = null, bool? allowAxisOverride = null)
    {
        _locked = locked;

        if (locked)
        {
            // 허용 마스크 계산
            _allowMaskWhenLocked = 0;
            if (allowActions != null)
            {
                foreach (var a in allowActions)
                    _allowMaskWhenLocked |= (1 << (int)a);
            }
            else
            {
                // 기본: 아무 버튼도 허용 안 함
                _allowMaskWhenLocked = 0;
            }

            if (allowAxisOverride.HasValue)
                allowAxisWhileLocked = allowAxisOverride.Value;

            // 잠그는 순간 프레임을 초기화해 잔여 엣지 방지
            CurrentFrame = default;
        }
        else
        {
            // 해제 시 축 허용 옵션은 원래 serialize 값 유지(override를 해제)
            // 다음 프레임부터 정상 입력 수집
        }
    }

    /// <summary>토글 편의 함수</summary>
    public void ToggleLockInput(InputAction[] allowActions = null, bool? allowAxisOverride = null)
        => LockInput(!_locked, allowActions, allowAxisOverride);

    /// <summary>잠금 + 특정 버튼만 허용(가독성용)</summary>
    public void LockAllowOnly(params InputAction[] allow) => LockInput(true, allow, false);

    /// <summary>잠금 해제</summary>
    public void Unlock() => LockInput(false);
}
