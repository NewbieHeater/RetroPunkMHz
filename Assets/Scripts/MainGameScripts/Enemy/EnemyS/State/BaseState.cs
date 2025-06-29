public abstract class BaseState
{
    protected EnemyFSMBase enemy;
    protected bool stop;

    public BaseState(EnemyFSMBase enemy)
    {
        this.enemy = enemy;
    }

    public abstract void OperateEnter();
    public abstract void OperateUpdate();
    public abstract void OperateExit();
    public abstract void OperateFixedUpdate();
}