using UnityEngine;

[RequireComponent(typeof(Recordable))]
public class Box : MonoBehaviour, IAttackable
{
    public Invoker invoker;
    Recordable rec;

    [Header("발사 세팅")]
    public float forceMagnitude = 5f;
    public Vector3 forceDirection = Vector3.up + Vector3.right;

    void OnEnable()
    {
        rec = GetComponent<Recordable>();
        invoker = GameObject.Find("Invoker").GetComponent<Invoker>();
    }

    public void TakeDamage(in DamageInfo info)
    {
        // 1) 원본 파괴
        var destroyCmd = new DestroyCommand(rec.InstanceID);
        invoker.Record(destroyCmd);

        // 2) 새 박스 스폰
        var spawnCmd = new SpawnCommand(
            rec.InstanceID,
            rec.prefabPath,
            transform.position - GameManager.Instance.player.transform.position,
            transform.rotation
        );
        invoker.Record(spawnCmd);

        // 3) 새 박스에 힘 가하기
        Vector3 launchForce = forceDirection.normalized * forceMagnitude;
        var rb = this.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(launchForce, ForceMode.Impulse);
        //var throwCmd = new ThrowCommand(rec.InstanceID, launchForce);
        //invoker.RecordAndExecute(throwCmd);
    }
}
