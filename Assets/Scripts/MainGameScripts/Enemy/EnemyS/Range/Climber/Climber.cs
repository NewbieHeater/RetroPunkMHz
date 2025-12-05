/*
 * using UnityEngine;

public class Climber : EnemyBase
{
    [Header("Attack/Hit")]
    [SerializeField] private BoxCollider _meleeAttackCollider;
    [SerializeField, Min(0f)] private float _windup = 0.4f;
    [SerializeField, Min(0f)] private float _strike = 0.2f;
    [SerializeField, Min(0f)] private float _cooldown = 0.5f;
    [SerializeField] private float _knockbackStrength = 4f;
    [SerializeField] private float _hitStun = 0.25f;

    private float _t;
    private bool _strikeOpened;

    private enum APhase { Windup, Strike, Cooldown }
    APhase _phase;

    protected override void OnEnable()
    {
        base.OnEnable();
        _state = StateInfo.Idle;
        OnEnterState(StateInfo.Idle);
    }

    // ====== FSM 구현 ======
    protected override void TickIdle()
    {
        base.TickIdle(); // 기본: 플레이어 보이면 Attack, 아니면 Move
        // 필요 시 추가 행동
    }

    protected override void TickMove()
    {
        PatrolTick(RigidNavigation.MoveMode.Climb);
        if (IsPlayerInSight(_attackRange))
        {
            _nav.isStopped = true;
            SetState(StateInfo.Attack);
        }
    }

    protected override void TickAttack()
    {
        _t -= Time.deltaTime;

        switch (_phase)
        {
            case APhase.Windup:
                FacePlayerY();
                if (_t <= 0f) BeginPhase(APhase.Strike, _strike);
                break;

            case APhase.Strike:
                if (!_strikeOpened)
                {
                    ToggleMelee(true);
                    _strikeOpened = true;
                }
                if (_t <= 0f)
                {
                    ToggleMelee(false);
                    BeginPhase(APhase.Cooldown, _cooldown);
                }
                break;

            case APhase.Cooldown:
                if (_t <= 0f)
                {
                    if (IsPlayerInSight(_attackRange)) SetState(StateInfo.Attack);
                    else if (IsPlayerInSight(_aggroRange)) SetState(StateInfo.Move);
                    else SetState(StateInfo.Idle);
                }
                break;
        }
    }

    protected override void TickHit()
    {
        _t -= Time.deltaTime;

        // 플레이어 반대 방향으로 고정 넉백(있으면 Rigidbody, 없으면 폴백)
        if (_player)
        {
            float dir = Mathf.Sign(transform.position.x - _player.transform.position.x);
            Vector3 kb = new Vector3(dir * _knockbackStrength, 0f, 0f);

            if (_rigid) _rigid.velocity = new Vector3(kb.x, _rigid.velocity.y, 0f);
            else transform.position += kb * Time.deltaTime;
        }

        if (_t <= 0f)
        {
            if (IsPlayerInSight(_attackRange)) SetState(StateInfo.Attack);
            else SetState(StateInfo.Move);
        }
    }

    protected override void OnEnterState(StateInfo s)
    {
        base.OnEnterState(s);
        switch (s)
        {
            case StateInfo.Idle:
                _animator.Play("Idle", 0, 0f);
                _t = 0f;
                ToggleMelee(false);
                break;

            case StateInfo.Move:
                BeginPatrol(RigidNavigation.MoveMode.Climb);
                _animator.CrossFade("Move", 0.10f, 0);
                ToggleMelee(false);
                break;

            case StateInfo.Attack:
                _animator.CrossFade("Attack", 0.10f, 0);
                BeginPhase(APhase.Windup, _windup);
                ToggleMelee(false);
                break;

            case StateInfo.Hit:
                _animator.CrossFade("Hit", 0.05f, 0);
                _t = _hitStun;
                ToggleMelee(false);
                break;

            case StateInfo.Death:
                _animator.Play("Death", 0, 0f);
                ToggleMelee(false);
                break;
        }
    }

    protected override void OnExitState(StateInfo s)
    {
        base.OnExitState(s);
        if (s == StateInfo.Move) _nav.isStopped = true;
        if (s == StateInfo.Attack) ToggleMelee(false);
    }

    // ====== Helpers ======
    void ToggleMelee(bool on)
    {
        if (_meleeAttackCollider && _meleeAttackCollider.enabled != on)
            _meleeAttackCollider.enabled = on;
    }

    void BeginPhase(APhase p, float dur)
    {
        _phase = p;
        _t = Mathf.Max(0f, dur);
        if (p != APhase.Strike) _strikeOpened = false;
    }

    void FacePlayerY()
    {
        if (!_player) return;
        Vector3 fwd = _player.transform.position - transform.position;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) return;
        // 필요 시 회전 구현(현재는 연출 간소화)
        // var look = Quaternion.LookRotation(fwd);
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 720f * Time.deltaTime);
    }

    public override void TakeDamage(in DamageInfo info)
    {
        base.TakeDamage(info);
        if (_state != StateInfo.Death)
            SetState(StateInfo.Hit);
    }
}

 * */