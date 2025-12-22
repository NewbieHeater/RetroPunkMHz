public abstract class EnemyState<TOwner> : IEnemyState where TOwner : EnemyBase
{
    protected readonly TOwner owner;
    protected readonly EnemyStateMachine fsm;

    protected EnemyState(TOwner owner, EnemyStateMachine fsm)
    {
        this.owner = owner;
        this.fsm = fsm;
    }

    public abstract StateInfo Kind { get; }

    public virtual void Enter() { }
    public virtual void Tick(float deltaTime) { }
    public virtual void Exit() { }
}
