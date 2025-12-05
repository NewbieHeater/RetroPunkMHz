using UnityEngine;

public class EnemyRobot : EnemyBase
{
    [Header("Melee / Hit")]
    [SerializeField] private BoxCollider _meleeAttackCollider;
    [SerializeField] private float knockbackStrength = 3f;
    [SerializeField] private float hitStun = 0.5f;

    [Header("Animator States / Params")]
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string moveState = "Move";
    [SerializeField] private string attackTrig = "Attack";
    [SerializeField] private string attackTag = "Attack";
    [SerializeField] private string hitState = "Hit";
    [SerializeField] private string deathState = "Death";
    [SerializeField] private string movingBool = "Moving";

    public float KnockbackStrength => knockbackStrength;
    public float HitStun => hitStun;

    public string IdleStateName => idleState;
    public string MoveStateName => moveState;
    public string HitStateName => hitState;
    public string DeathStateName => deathState;
    public string AttackTrigger => attackTrig;
    public string AttackTag => attackTag;
    public string MovingBoolName => movingBool;

    public BoxCollider MeleeCollider => _meleeAttackCollider;

    /// <summary>애니메이터 Moving bool과 Nav.isStopped를 동시에 관리</summary>
    public void SetMoving(bool on)
    {
        if (Anim)
            Anim.SetBool(movingBool, on);
        if (Nav)
            Nav.isStopped = !on;
    }

    public void ToggleMelee(bool on)
    {
        if (_meleeAttackCollider && _meleeAttackCollider.enabled != on)
            _meleeAttackCollider.enabled = on;
    }

    public bool IsInOrTransitionToAttack()
    {
        if (!Anim) return false;

        var st = Anim.GetCurrentAnimatorStateInfo(0);
        if (st.IsTag(attackTag)) return true;

        if (Anim.IsInTransition(0))
        {
            var next = Anim.GetNextAnimatorStateInfo(0);
            if (next.IsTag(attackTag)) return true;
        }

        return false;
    }

    /// <summary>이 적이 사용할 상태들을 등록</summary>
    protected override void RegisterStates(EnemyStateMachine fsm)
    {
        fsm.Register(new RobotIdleState(this, fsm));
        fsm.Register(new RobotMoveState(this, fsm));
        fsm.Register(new RobotAttackState(this, fsm));
        fsm.Register(new RobotHitState(this, fsm));
        fsm.Register(new RobotDeathState(this, fsm));
    }

    /// <summary>추가적인 사망 연출이 필요하면 여기 override</summary>
    protected override void OnDie(in DamageInfo info)
    {
        base.OnDie(info);
        // 예: 일정 시간 후 풀로 반환 등
        Destroy(gameObject, 2f);
    }
}
