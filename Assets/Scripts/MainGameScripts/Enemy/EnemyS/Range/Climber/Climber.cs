using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climber : EnemyBase
{
    [Header("Attack/Hit")]
    [SerializeField] private BoxCollider _meleeAttackCollider;
    [SerializeField, Min(0f)] private float _windup = 0.4f;
    [SerializeField, Min(0f)] private float _strike = 0.2f;
    [SerializeField, Min(0f)] private float _cooldown = 0.5f;
    [SerializeField] private float _knockbackStrength = 4f;
    [SerializeField] private float _hitStun = 0.25f;

    private float _t;                   // 공용 타이머
    private APhase _phase;              // 공격 페이즈
    private bool _strikeOpened;         // 콜라이더 열림 여부
    private Rigidbody _rb;              // 넉백용(있으면 사용)

    private enum APhase { Windup, Strike, Cooldown }

    protected override void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody>();
        _state = StateInfo.Idle;
        EnterState(StateInfo.Idle);
    }

    protected override void FSM()
    {
        switch (_state)
        {
            case StateInfo.Idle: TickIdle(); break;
            case StateInfo.Move: TickMove(); break;
            case StateInfo.Attack: TickAttack(); break;
            case StateInfo.Hit: TickHit(); break;
            case StateInfo.Death: break;
        }
    }

    #region State Machine

    protected override void SetState(StateInfo next)
    {
        if (_state == next) return;
        ExitState(_state);
        _state = next;
        EnterState(_state);
    }

    protected override void EnterState(StateInfo s)
    {
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

    protected override void ExitState(StateInfo s)
    {
        // 상태별 종료 처리 최소화
        if (s == StateInfo.Move) _nav.isStopped = true;
        if (s == StateInfo.Attack) ToggleMelee(false);
    }

    #endregion

    #region Ticks

    private void TickIdle()
    {
        _t += Time.deltaTime;

        if (IsPlayerInSight(_aggroRange))
        {
            SetState(StateInfo.Attack);
            return;
        }

        if (_t > 1.2f) SetState(StateInfo.Move);
    }

    private void TickMove()
    {
        Tick(RigidNavigation.MoveMode.Climb);

        if (IsPlayerInSight(_attackRange))
        {
            _nav.isStopped = true;
            SetState(StateInfo.Attack);
        }
    }

    private void TickAttack()
    {
        _t -= Time.deltaTime;

        switch (_phase)
        {
            case APhase.Windup:
                FacePlayerY();
                if (InPhaseEnded())
                    BeginPhase(APhase.Strike, _strike);
                break;

            case APhase.Strike:
                // 히트판정 1회 오픈
                if (!_strikeOpened)
                {
                    ToggleMelee(true);
                    _strikeOpened = true;
                }

                if (InPhaseEnded())
                {
                    ToggleMelee(false);
                    BeginPhase(APhase.Cooldown, _cooldown);
                }
                break;

            case APhase.Cooldown:
                if (InPhaseEnded())
                {
                    if (IsPlayerInSight(_attackRange))
                        SetState(StateInfo.Attack); // 연속 공격
                    else if (IsPlayerInSight(_aggroRange))
                        SetState(StateInfo.Move);   // 재배치
                    else
                        SetState(StateInfo.Idle);
                }
                break;
        }
    }

    private void TickHit()
    {
        _t -= Time.deltaTime;

        // 거리 무관 고정 넉백 (플레이어 반대 방향)
        if (_player)
        {
            float dir = Mathf.Sign(transform.position.x - _player.transform.position.x);
            Vector3 kb = new Vector3(dir * _knockbackStrength, 0f, 0f);

            if (_rb)
            {
                // 델타타임 고려한 가벼운 속도 변화
                _rb.velocity = new Vector3(kb.x, _rb.velocity.y, 0f);
            }
            else
            {
                transform.position += kb * Time.deltaTime;
            }
        }

        if (_t <= 0f)
        {
            if (IsPlayerInSight(_attackRange)) SetState(StateInfo.Attack);
            else SetState(StateInfo.Move);
        }
    }

    #endregion

    #region Utils & Helpers

    private void FacePlayerY()
    {
        if (!_player) return;
        Vector3 fwd = _player.transform.position - transform.position;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) return;

        const float turnDegPerSec = 720f;
        Quaternion look = Quaternion.LookRotation(fwd);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnDegPerSec * Time.deltaTime);
    }

    private void ToggleMelee(bool on)
    {
        if (_meleeAttackCollider && _meleeAttackCollider.enabled != on)
            _meleeAttackCollider.enabled = on;
    }

    private void BeginPhase(APhase p, float dur)
    {
        _phase = p;
        _t = Mathf.Max(0f, dur);
        if (p != APhase.Strike) _strikeOpened = false;
    }

    private bool InPhaseEnded() => _t <= 0f;

    public override void TakeDamage(in DamageInfo info)
    {
        base.TakeDamage(info);
        if (_state != StateInfo.Death)
            SetState(StateInfo.Hit);
    }

    #endregion
}
