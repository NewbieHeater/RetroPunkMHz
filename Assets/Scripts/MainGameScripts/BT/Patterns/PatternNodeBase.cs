using UnityEngine;

public abstract class PatternNodeBase : IPatternNode
{
    // 공통 의존성
    protected readonly Transform owner;        // 보스 또는 기준점
    protected readonly Transform target;       // 보통 플레이어
    protected readonly LayerMask groundMask;

    // 공통 상태
    protected bool running;
    protected float timer;

    protected PatternNodeBase(Transform owner, Transform target, LayerMask groundMask)
    {
        this.owner = owner;
        this.target = target;
        this.groundMask = groundMask;
    }

    // 기본 구현: 필요 시 오버라이드
    public virtual void NodeStart(BTContext ctx)
    {
        running = true;
        timer = 0f;
    }

    public abstract NodeStatus Tick(BTContext ctx);

    public virtual void NodeStop(BTContext ctx, NodeStatus result)
    {
        running = false;
    }

    // ------- 공통 유틸 -------
    protected Vector3 GetTargetXZOrOwner()
    {
        Vector3 p = target ? target.position : (owner ? owner.position : Vector3.zero);
        p.y = 0f;
        return p;
    }

    protected Vector3 SnapToGround(Vector3 xz, float up = 20f, float down = 60f)
    {
        Vector3 from = new Vector3(xz.x, xz.y + up, xz.z);
        return Physics.Raycast(from, Vector3.down, out var hit, up + down, groundMask, QueryTriggerInteraction.Ignore)
             ? hit.point
             : new Vector3(xz.x, xz.y, xz.z); // 폴백
    }
}
