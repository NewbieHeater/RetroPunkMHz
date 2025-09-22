using UnityEngine;


public struct PlayerInputFrame
{
    public float moveX;

    public bool jumpDown;
    public bool jumpUp;

    public bool sprintToggleDown;

    public bool interactDown;    // 상호작용(E)
    public bool attackDown;      // 필요시
    public bool attackHeld;      // 필요시
}


public class GlobalInputRouter : Singleton<GlobalInputRouter>
{

    [Header("Bindings")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";
    public KeyCode sprintToggleKey = KeyCode.LeftShift;
    public KeyCode interactKey = KeyCode.E;
    public int attackMouseButton = 0; // 좌클릭

    private bool _locked;
    private PlayerInputFrame _current;

    void Update()
    {
        if (_locked)
        {
            _current = default;
            return;
        }

        // 아날로그/축
        _current.moveX = Input.GetAxisRaw(horizontalAxis);

        // 점프
        _current.jumpDown = Input.GetButtonDown(jumpButton);
        _current.jumpUp = Input.GetButtonUp(jumpButton);

        // 스프린트 토글
        _current.sprintToggleDown = Input.GetKeyDown(sprintToggleKey);

        // 상호작용
        _current.interactDown = Input.GetKeyDown(interactKey);

        // 공격(옵션)
        _current.attackDown = Input.GetMouseButtonDown(attackMouseButton);
        _current.attackHeld = Input.GetMouseButton(attackMouseButton);
    }

    public PlayerInputFrame GetFrame() => _current;

    public void LockInput(bool locked) => _locked = locked;

    public void TogleLockInput() => _locked = !_locked;
}
