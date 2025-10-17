using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climber : EnemyBase
{
    [Header("Attack/Hit")]
    [SerializeField] private BoxCollider _meleeAttackCollider;
    [SerializeField] private float _windup = 0.4f;
    [SerializeField] private float _strike = 0.2f;
    [SerializeField] private float _cooldown = 0.5f;
    [SerializeField] private float _knockbackStrength = 4f;
    [SerializeField] private float _hitStun = 0.25f;

    private float _timer;

    // Attack 내부 단계
    private enum APhase { Windup, Strike, Cooldown }
    private APhase _aphase;
    private bool _strikeFired;


    protected override void Start()
    {
        base.Start();
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

    #region StateChange
    protected override void SetState(StateInfo next)
    {
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
                _timer = 0f;
                break;

            case StateInfo.Move:
                _animator.CrossFade("Move", 0.1f, 0);
                _patrolController?.Enter();
                break;

            case StateInfo.Attack:
                _animator.CrossFade("Attack", 0.1f, 0);
                _aphase = APhase.Windup;
                _timer = _windup;
                _strikeFired = false;
                if (_meleeAttackCollider) _meleeAttackCollider.enabled = false;
                _patrolController?.Exit(); // 이동 중지
                break;

            case StateInfo.Hit:
                _animator.CrossFade("Hit", 0.05f, 0);
                _timer = _hitStun;
                _patrolController?.Exit();
                if (_meleeAttackCollider) _meleeAttackCollider.enabled = false;
                break;

            case StateInfo.Death:
                _animator.Play("Death");
                _patrolController?.Exit();
                if (_meleeAttackCollider) _meleeAttackCollider.enabled = false;
                break;
        }
    }

    protected override void ExitState(StateInfo s)
    {
        switch (s)
        {
            case StateInfo.Move:
                _patrolController?.Exit();
                break;
            case StateInfo.Attack:
                if (_meleeAttackCollider) _meleeAttackCollider.enabled = false;
                break;
        }
    }

    #endregion

    // ===== Ticks =====
    private void TickIdle()
    {
        _timer += Time.deltaTime;

        if (IsPlayerInSight(_aggroRange))
        {
            SetState(StateInfo.Attack);
            return;
        }
        if (_timer > 1.2f) // 짧게 머물다 순찰로
        {
            SetState(StateInfo.Move);
        }
    }

    private void TickMove()
    {
        _patrolController?.Tick();

        if (IsPlayerInSight(_attackRange))
        {
            _patrolController?.Exit();
            SetState(StateInfo.Attack); // !!! Idle 아니고 Attack으로
            return;
        }
    }

    private void TickAttack()
    {
        _timer -= Time.deltaTime;

        switch (_aphase)
        {
            case APhase.Windup:
                FacePlayerY();
                if (_timer <= 0f)
                {
                    _aphase = APhase.Strike;
                    _timer = _strike;
                    _strikeFired = false;
                }
                break;

            case APhase.Strike:
                if (!_strikeFired)
                {
                    // 히트판정 구간 열기
                    if (_meleeAttackCollider) _meleeAttackCollider.enabled = true;
                    _strikeFired = true;
                }
                if (_timer <= 0f)
                {
                    if (_meleeAttackCollider) _meleeAttackCollider.enabled = false;
                    _aphase = APhase.Cooldown;
                    _timer = _cooldown;
                }
                break;

            case APhase.Cooldown:
                if (_timer <= 0f)
                {
                    // 상황에 따라 다음 상태 결정
                    if (IsPlayerInSight(_attackRange))
                        SetState(StateInfo.Attack); // 연속 공격 허용
                    else if (IsPlayerInSight(_aggroRange))
                        SetState(StateInfo.Move);   // 추적/재배치
                    else
                        SetState(StateInfo.Idle);
                }
                break;
        }
    }

    private void TickHit()
    {
        _timer -= Time.deltaTime;

        // 거리 무관 고정 넉백
        float dir = Mathf.Sign(transform.position.x - _player.transform.position.x);
        Vector3 kb = new Vector3(dir * _knockbackStrength, 0f, 0f);
        transform.position += kb * Time.deltaTime;

        if (_timer <= 0f)
        {
            // 시야에 있으면 바로 공격, 아니면 이동
            if (IsPlayerInSight(_attackRange)) SetState(StateInfo.Attack);
            else SetState(StateInfo.Move);
        }
    }

    // ===== Utils =====
    private void FacePlayerY()
    {
        if (!_player) return;
        Vector3 fwd = (_player.transform.position - transform.position);
        fwd.y = 0f;
        if (fwd.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(fwd), 720f * Time.deltaTime);
    }

    public override void TakeDamage(in DamageInfo info)
    {
        base.TakeDamage(info);
        if (_state != StateInfo.Death)
            SetState(StateInfo.Hit); // 반드시 SetState로
    }
}
