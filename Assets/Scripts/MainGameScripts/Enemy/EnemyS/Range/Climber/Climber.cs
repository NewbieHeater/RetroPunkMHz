using UnityEngine;

public class Climber : EnemyBase
{
    [Header("Attack / Hit")]
    [SerializeField] private BoxCollider _meleeAttackCollider;

    [SerializeField, Min(0f)] private float _windup = 0.4f;
    [SerializeField, Min(0f)] private float _strike = 0.2f;
    [SerializeField, Min(0f)] private float _cooldown = 0.5f;

    [SerializeField] private float _knockbackStrength = 4f;
    [SerializeField] private float _hitStun = 0.25f;

    [Header("Animator State Names (Clips)")]
    [SerializeField] private string _idleState = "Idle";
    [SerializeField] private string _moveState = "Move";
    [SerializeField] private string _attackState = "Attack";
    [SerializeField] private string _hitState = "Hit";
    [SerializeField] private string _deathState = "Death";

    [Header("Animator Params (Optional)")]
    [Tooltip("없으면 빈 문자열로 두세요. (파라미터 없는데 값 넣으면 Animator 에러 로그 납니다)")]
    [SerializeField] private string _movingBool = "";

    [Header("CrossFade Times")]
    [SerializeField, Min(0f)] private float _moveCrossFade = 0.10f;
    [SerializeField, Min(0f)] private float _attackCrossFade = 0.10f;
    [SerializeField, Min(0f)] private float _hitCrossFade = 0.05f;

    // ====== Expose (States에서 접근) ======
    public BoxCollider MeleeCollider => _meleeAttackCollider;

    public float Windup => _windup;
    public float Strike => _strike;
    public float Cooldown => _cooldown;

    public float KnockbackStrength => _knockbackStrength;
    public float HitStun => _hitStun;

    public string IdleStateName => _idleState;
    public string MoveStateName => _moveState;
    public string AttackStateName => _attackState;
    public string HitStateName => _hitState;
    public string DeathStateName => _deathState;

    public string MovingBoolName => _movingBool;

    public float MoveCrossFade => _moveCrossFade;
    public float AttackCrossFade => _attackCrossFade;
    public float HitCrossFade => _hitCrossFade;

    // 원래 Climber는 Hit에서 Rigidbody 있으면 velocity로 넉백, 없으면 transform 폴백을 했음
    public Rigidbody RigidBody => _rigid;

    protected override void Awake()
    {
        base.Awake();

        // 인스펙터 지정 우선, 없으면 자동 탐색
        if (!_meleeAttackCollider)
            _meleeAttackCollider = GetComponentInChildren<BoxCollider>(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // 풀링/비활성화 시 콜라이더 켜진 채로 남는 것 방지
        ToggleMelee(false);
        SetMoving(false);
    }

    /// <summary>Animator Moving bool과 Nav.isStopped를 같이 관리(원하면 사용)</summary>
    public void SetMoving(bool on)
    {
        if (Anim && !string.IsNullOrEmpty(_movingBool))
            Anim.SetBool(_movingBool, on);

        if (Nav)
            Nav.isStopped = !on;
    }

    public void ToggleMelee(bool on)
    {
        if (_meleeAttackCollider && _meleeAttackCollider.enabled != on)
            _meleeAttackCollider.enabled = on;
    }

    /// <summary>기존 Climber의 IsPlayerInSight(_range) 대체</summary>
    public bool IsPlayerInSight(float range)
    {
        if (!Player || !Sight) return false;
        return Sight.IsTargetInSight(Player.transform, range);
    }

    /// <summary>원본 코드의 FacePlayerY (연출 간소화 유지)</summary>
    public void FacePlayerY()
    {
        if (!Player) return;

        Vector3 fwd = Player.transform.position - transform.position;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) return;

        // 필요 시 회전 구현
        // var look = Quaternion.LookRotation(fwd);
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 720f * Time.deltaTime);
    }

    /// <summary>원하면 애니메이션 이벤트로도 콜라이더 토글 가능</summary>
    public void AttackOn() => ToggleMelee(true);
    public void AttackEnd() => ToggleMelee(false);

    /// <summary>이 적이 사용할 상태들을 등록</summary>
    protected override void RegisterStates(EnemyStateMachine fsm)
    {
        fsm.Register(new ClimberIdleState(this, fsm));
        fsm.Register(new ClimberMoveState(this, fsm));
        fsm.Register(new ClimberAttackState(this, fsm));
        fsm.Register(new ClimberHitState(this, fsm));
        fsm.Register(new ClimberDeathState(this, fsm));
    }
}
