using UnityEngine;

public interface IPlayerInput
{
    float MoveX { get; }
    bool JumpDown { get; }
    bool JumpUp { get; }
    bool SprintToggleDown { get; }

    /// Call once per Update before controllers query values.
    void Read();
}

public class KeyboardInput : MonoBehaviour, IPlayerInput
{
    [Header("Bindings")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";
    public KeyCode sprintToggleKey = KeyCode.LeftShift;

    private float _moveX;
    private bool _jumpDown;
    private bool _jumpUp;
    private bool _sprintToggleDown;

    public float MoveX => _moveX;
    public bool JumpDown => _jumpDown;
    public bool JumpUp => _jumpUp;
    public bool SprintToggleDown => _sprintToggleDown;

    public void Read()
    {
        _moveX = Input.GetAxisRaw(horizontalAxis);

        // Support both legacy input button and key (defensive).
        _jumpDown = Input.GetButtonDown(jumpButton);
        _jumpUp = Input.GetButtonUp(jumpButton);

        if (Input.GetKeyDown(sprintToggleKey))
            _sprintToggleDown = true;
        else
            _sprintToggleDown = false;
    }
}
