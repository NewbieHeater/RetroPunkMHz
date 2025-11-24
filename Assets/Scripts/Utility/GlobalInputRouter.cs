using UnityEngine;

namespace Game.Controls
{
    // 새 Input System의 UnityEngine.InputSystem.InputAction 과 이름 충돌 방지
    public enum GameInputAction
    {
        Jump,
        SprintToggle,
        Interact,
        Attack,
        Charge,
        InventoryToggle 
    }


    public struct FrameButtons
    {
        private readonly int _downMask;
        private readonly int _heldMask;
        private readonly int _upMask;

        public FrameButtons(int downMask, int heldMask, int upMask)
        {
            _downMask = downMask;
            _heldMask = heldMask;
            _upMask = upMask;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsDown(GameInputAction a) { int b = 1 << (int)a; return (_downMask & b) != 0; }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsHeld(GameInputAction a) { int b = 1 << (int)a; return (_heldMask & b) != 0; }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsUp(GameInputAction a) { int b = 1 << (int)a; return (_upMask & b) != 0; }

        public int RawDownMask => _downMask;
        public int RawHeldMask => _heldMask;
        public int RawUpMask => _upMask;

        public bool AnyDown => _downMask != 0;
        public bool AnyHeld => _heldMask != 0;
        public bool AnyUp => _upMask != 0;

        // 허용 마스크만 남김
        public FrameButtons FilterByAllowMask(int allowMask)
        {
            return new FrameButtons(_downMask & allowMask, _heldMask & allowMask, _upMask & allowMask);
        }

        public static int MaskOf(params Game.Controls.GameInputAction[] actions)
        {
            int m = 0; foreach (var a in actions) m |= 1 << (int)a; return m;
        }
    }

    public struct PlayerInputFrame
    {
        public readonly int frameCount; // 수집된 Unity Time.frameCount
        public readonly float moveX;    // 축
        public readonly FrameButtons buttons;

        public PlayerInputFrame(int frameCount, float moveX, FrameButtons buttons)
        {
            this.frameCount = frameCount;
            this.moveX = moveX;
            this.buttons = buttons;
        }
    }

    /// <summary>
    /// 입력 라우터(레거시 Input Manager 기반).
    /// - Update에서 입력을 수집하고 링버퍼에 저장
    /// - FixedUpdate 소비를 위해 프레임 합성 API 제공(GetFrameForFixedUpdate)
    /// - 입력 잠금/허용, 잠금 중 엣지 큐잉 옵션 제공
    /// </summary>
    [DefaultExecutionOrder(-80)]
    public class GlobalInputRouter : Singleton<GlobalInputRouter>
    {
        [Header("Bindings (Legacy Input Manager names)")]
        public string horizontalAxis = "Horizontal";
        public string jumpButton = "Jump";
        public string sprintToggleButton = "SprintToggle";
        public string interactButton = "Interact";
        public string attackButton = "Fire1";
        public string chargeButton = "Fire2";
        public string inventoryToggleButton = "InventoryToggle"; 


        [Header("Lock Options")]
        [Tooltip("입력 잠금 시에도 축 입력을 허용할지(기본 false)")]
        [SerializeField] private bool allowAxisWhileLocked = false;

        public enum LockedDownEdgeMode { Drop, QueueAllowed, QueueAll }

        [Tooltip("잠금 중 발생한 Down 엣지 처리 방식")]
        public LockedDownEdgeMode lockedDownEdgeMode = LockedDownEdgeMode.Drop;

        [Header("FixedUpdate Bridge")]
        [Tooltip("FixedUpdate 간격 동안 Update가 여러 번 돌 때, Down/Up 유실 방지를 위해 링버퍼 크기를 설정합니다.")]
        [Range(4, 64)] public int ringBufferSize = 16;

        public bool IsLocked => _locked;
        public PlayerInputFrame CurrentFrame { get; private set; } // 마지막 Update 수집 프레임

        // --- 내부 상태 ---
        private bool _locked;
        private int _allowMaskWhenLocked;

        // allowAxisWhileLocked 오버라이드 복원용
        private bool _serializedAllowAxisWhileLocked;
        private bool _axisOverrideActive;

        // 잠금 중 눌린 Down 엣지 큐
        private int _queuedDownMask;
        private int _injectDownNextUpdate; // Unlock 후 다음 Update에 1프레임 주입

        // 링버퍼(최근 Update 프레임들)
        private PlayerInputFrame[] _frames;
        private int[] _framesFC; // frameCount
        private int _head = -1;  // 마지막으로 쓴 위치
        private int _lastFixedConsumedFC; // FixedUpdate에서 마지막으로 소비한 frameCount

        protected override void Awake()
        {
            _serializedAllowAxisWhileLocked = allowAxisWhileLocked;
            AllocateRingBuffer();
            base.Awake();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && (_frames == null || _frames.Length != ringBufferSize))
                AllocateRingBuffer();
        }

        private void AllocateRingBuffer()
        {
            _frames = new PlayerInputFrame[ringBufferSize];
            _framesFC = new int[ringBufferSize];
            _head = -1;
            _lastFixedConsumedFC = 0;
        }

        private void PushFrame(in PlayerInputFrame f)
        {
            _head = (_head + 1) % ringBufferSize;
            _frames[_head] = f;
            _framesFC[_head] = f.frameCount;
        }

        private void Update()
        {
            int down = 0, held = 0, up = 0;

            // 1) 원시 버튼 수집
            AccumulateButton(jumpButton, GameInputAction.Jump, ref down, ref held, ref up);
            AccumulateButton(sprintToggleButton, GameInputAction.SprintToggle, ref down, ref held, ref up);
            AccumulateButton(interactButton, GameInputAction.Interact, ref down, ref held, ref up);
            AccumulateButton(attackButton, GameInputAction.Attack, ref down, ref held, ref up);
            AccumulateButton(chargeButton, GameInputAction.Charge, ref down, ref held, ref up);
            AccumulateButton(inventoryToggleButton, GameInputAction.InventoryToggle, ref down, ref held, ref up);

            int rawDown = down; // 필터 전 Down 백업(QueueAll 용)

            // 2) 축 입력
            float moveX = 0f;
            if (!string.IsNullOrEmpty(horizontalAxis))
                moveX = Input.GetAxisRaw(horizontalAxis);

            // 3) 엣지/축 필터링
            FrameButtons fb = new FrameButtons(down, held, up);

            if (_locked)
            {
                fb = fb.FilterByAllowMask(_allowMaskWhenLocked);
                if (!allowAxisWhileLocked) moveX = 0f;

                // 잠금 중 Down 엣지 큐잉
                if (lockedDownEdgeMode == LockedDownEdgeMode.QueueAllowed)
                    _queuedDownMask |= fb.RawDownMask;                 // 허용된 것만 큐
                else if (lockedDownEdgeMode == LockedDownEdgeMode.QueueAll)
                    _queuedDownMask |= rawDown;                        // 허용 여부 무시하고 전부 큐
            }

            // 4) Unlock 직후 1프레임 Down 주입
            if (_injectDownNextUpdate != 0)
            {
                fb = new FrameButtons(fb.RawDownMask | _injectDownNextUpdate, fb.RawHeldMask, fb.RawUpMask);
                _injectDownNextUpdate = 0;
            }

            // 5) CurrentFrame 및 링버퍼 기록
            CurrentFrame = new PlayerInputFrame(Time.frameCount, moveX, fb);
            PushFrame(CurrentFrame);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Alt-Tab 등으로 포커스 이탈 시 잔여 엣지 제거
                CurrentFrame = default;
                _queuedDownMask = 0;
                _injectDownNextUpdate = 0;
            }
        }

        private void AccumulateButton(string buttonName, GameInputAction action, ref int down, ref int held, ref int up)
        {
            if (string.IsNullOrEmpty(buttonName)) return;
            int bit = 1 << (int)action;
            if (Input.GetButtonDown(buttonName)) down |= bit;
            if (Input.GetButton(buttonName)) held |= bit;
            if (Input.GetButtonUp(buttonName)) up |= bit;
        }

        /// <summary>
        /// FixedUpdate 소비용: 마지막으로 소비한 이후의 Update 프레임들을 합성해 반환합니다.
        /// - Down/Up: OR 합성(엣지 유실 방지)
        /// - Held/Axis: 가장 최근 Update 프레임 기준
        /// - 새 프레임이 없다면 Down/Up=0, Held/Axis는 마지막 상태 유지
        /// </summary>
        public PlayerInputFrame GetFrameForFixedUpdate()
        {
            int newestFC = 0;
            int aggDown = 0, aggUp = 0;
            int lastHeld = 0;
            float lastMoveX = 0f;
            bool foundNew = false;

            // newest -> oldest로 스캔하며 lastHeld/lastMoveX는 가장 최신 프레임으로 갱신
            for (int n = 0; n < ringBufferSize; n++)
            {
                if (_head < 0) break;
                int idx = (_head - n + ringBufferSize) % ringBufferSize;
                int fc = _framesFC[idx];
                if (fc == 0) break; // 비어있는 슬롯
                if (fc <= _lastFixedConsumedFC) break; // 이전에 소비했다면 종료

                var fr = _frames[idx];
                aggDown |= fr.buttons.RawDownMask;
                aggUp |= fr.buttons.RawUpMask;

                if (!foundNew || fc > newestFC)
                {
                    newestFC = fc;
                    lastHeld = fr.buttons.RawHeldMask;
                    lastMoveX = fr.moveX;
                    foundNew = true;
                }
            }

            if (!foundNew)
            {
                // 새 Update가 없으면 Down/Up은 0으로, Held/Axis는 마지막 상태 유지
                var last = (_head >= 0) ? _frames[_head] : default;
                return new PlayerInputFrame(Time.frameCount,
                    last.moveX,
                    new FrameButtons(0, last.buttons.RawHeldMask, 0));
            }

            _lastFixedConsumedFC = newestFC;
            return new PlayerInputFrame(Time.frameCount,
                    lastMoveX,
                    new FrameButtons(aggDown, lastHeld, aggUp));
        }

        /// <summary>
        /// 입력 잠금/해제. allowActions가 지정되면 잠금 중 해당 버튼만 통과.
        /// allowAxisOverride를 지정하면 잠금 동안만 임시로 축 허용 여부를 덮어씁니다.
        /// 해제 시 자동으로 serialize 원본 값으로 복원됩니다.
        /// </summary>
        public void LockInput(bool locked, GameInputAction[] allowActions = null, bool? allowAxisOverride = null)
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

                // 축 오버라이드 적용
                if (allowAxisOverride.HasValue)
                {
                    allowAxisWhileLocked = allowAxisOverride.Value;
                    _axisOverrideActive = true;
                }
                else
                {
                    _axisOverrideActive = false;
                }

                // 잠그는 순간 프레임 초기화(잔여 엣지 방지)
                CurrentFrame = default;
            }
            else
            {
                // 오버라이드 복원
                if (_axisOverrideActive)
                {
                    allowAxisWhileLocked = _serializedAllowAxisWhileLocked;
                    _axisOverrideActive = false;
                }

                // 잠금 중 큐잉된 Down 엣지 주입 예약
                if (_queuedDownMask != 0 && lockedDownEdgeMode != LockedDownEdgeMode.Drop)
                {
                    _injectDownNextUpdate = _queuedDownMask;
                    _queuedDownMask = 0;
                }
            }
        }

        /// <summary>잠금 + 특정 버튼만 허용(가독성용)</summary>
        public void LockAllowOnly(params GameInputAction[] allow) => LockInput(true, allow, false);

        /// <summary>토글 편의 함수</summary>
        public void ToggleLockInput(GameInputAction[] allowActions = null, bool? allowAxisOverride = null)
            => LockInput(!_locked, allowActions, allowAxisOverride);

        /// <summary>잠금 해제</summary>
        public void Unlock() => LockInput(false);
    }
}
