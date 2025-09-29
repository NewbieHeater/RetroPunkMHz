using UnityEditor;
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

    // 필요하면 전체 마스크 접근자도 노출 가능
    public int RawDownMask => _downMask;
    public int RawHeldMask => _heldMask;
    public int RawUpMask => _upMask;
}

public struct PlayerInputFrame
{
    // 축 입력도 함께 보관 (필요 없으면 제거 가능)
    public float moveX;
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
    public string sprintToggleButton = "SprintToggle"; // Project Settings > Input 에 등록해서 사용
    public string interactButton = "Interact";     // 동일
    public string attackButton = "Fire1";        // 좌클릭
    public string chargeButton = "Fire2";        // 우클릭

    public bool IsLocked => _locked;
    public PlayerInputFrame CurrentFrame { get; private set; }

    private bool _locked;


    void Update()
    {
        if (_locked)
        {
            // 잠금 중에는 모든 입력 0으로 고정 (엣지/홀드 모두 차단)
            CurrentFrame = default;
            return;
        }

        int down = 0, held = 0, up = 0;

        AccumulateButton(jumpButton, InputAction.Jump, ref down, ref held, ref up);
        AccumulateButton(sprintToggleButton, InputAction.SprintToggle, ref down, ref held, ref up);
        AccumulateButton(interactButton, InputAction.Interact, ref down, ref held, ref up);
        AccumulateButton(attackButton, InputAction.Attack, ref down, ref held, ref up);
        AccumulateButton(chargeButton, InputAction.Charge, ref down, ref held, ref up);

        // --- 축 입력(예: 좌우 이동) ---
        float moveX = 0f;
        if (!string.IsNullOrEmpty(horizontalAxis))
            moveX = Input.GetAxisRaw(horizontalAxis);

        FrameButtons fb = new FrameButtons(down, held, up);
        CurrentFrame = new PlayerInputFrame(moveX, fb);
    }

    /// <summary>
    /// Unity InputManager의 버튼 이름으로 Down/Held/Up을 읽어 비트 누적
    /// </summary>
    private void AccumulateButton(string buttonName, InputAction action, ref int down, ref int held, ref int up)
    {
        if (string.IsNullOrEmpty(buttonName)) return;

        int bit = 1 << (int)action;

        if (Input.GetButtonDown(buttonName)) down |= bit;
        if (Input.GetButton(buttonName)) held |= bit;
        if (Input.GetButtonUp(buttonName)) up |= bit;
    }

    /// <summary>
    /// 입력 잠금/해제. 잠그면 즉시 프레임을 초기화하여 잔여 엣지를 제거.
    /// </summary>
    public void LockInput(bool locked)
    {
        _locked = locked;
        if (locked)
        {
            // 잠그는 순간 즉시 클리어(스테이트 잔상 방지)
            CurrentFrame = default;
        }
        // 해제 시에는 다음 프레임에서 정상 입력을 읽음
    }

    public void ToggleLockInput() => LockInput(!_locked);
}
