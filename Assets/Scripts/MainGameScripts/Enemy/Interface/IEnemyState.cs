public interface IEnemyState
{
    StateInfo Kind { get; }

    /// <summary>상태 진입 시 한 번 호출</summary>
    void Enter();

    /// <summary>매 프레임 호출</summary>
    void Tick(float deltaTime);

    /// <summary>상태 종료 직전에 한 번 호출</summary>
    void Exit();
}
