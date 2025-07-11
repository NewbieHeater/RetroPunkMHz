using UnityEngine;

public class Box : MonoBehaviour, IAttackable
{
    [Header("녹화/재생용 Invoker")]
    public Invoker invoker;

    [Header("프리팹")]
    public GameObject boxPrefab;

    [Header("발사 설정")]
    public float forceMagnitude = 5f;
    public Vector3 forceDirection = Vector3.up + Vector3.right;

    // 이 메서드는 플레이어의 공격 콜백에서 호출된다고 가정
    public void OnHitByPlayer()
    {
        
    }

    public void TakeDamage(in DamageInfo info)
    {
        Debug.Log("attakc");
        // 1) 명령객체 생성
        var cmd = new ThrowCommand(
            boxPrefab: boxPrefab,
            spawnPosition: transform.position,
            spawnRotation: transform.rotation,
            launchForce: forceDirection.normalized * forceMagnitude,
            forceMode: ForceMode.Impulse
        );

        // 2) Invoker 에 실행 및 녹화 요청
        invoker.ExecuteCommand(cmd);

        // 3) 원래 상자는 녹화·실행 시에만 파괴
        if (!invoker.isReplaying)  // (IsReplaying 프로퍼티는 Invoker 에 구현해 두세요)
            Destroy(gameObject);
    }
}