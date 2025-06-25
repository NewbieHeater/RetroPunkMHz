public abstract class BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
{
    protected readonly TSelf enemy;
    public BaseState(TSelf ctx) => enemy = ctx;

    public virtual void OperateEnter() { }
    public virtual void OperateUpdate() { }
    public virtual void OperateExit() { }
    public virtual void OperateFixedUpdate() { }
}