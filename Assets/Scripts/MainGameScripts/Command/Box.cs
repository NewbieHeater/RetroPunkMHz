using UnityEngine;

[RequireComponent(typeof(Recordable))]
public class Box : MonoBehaviour, IAttackable
{
    public Invoker invoker;
    Recordable rec;

    [Header("�߻� ����")]
    public float forceMagnitude = 5f;
    public Vector3 forceDirection = Vector3.up + Vector3.right;

    void OnEnable()
    {
        rec = GetComponent<Recordable>();
        invoker = GameObject.Find("Invoker").GetComponent<Invoker>();
    }

    public void TakeDamage(in DamageInfo info)
    {
        // 1) ���� �ı�
        var destroyCmd = new DestroyCommand(rec.InstanceID);
        invoker.Record(destroyCmd);

        // 2) �� �ڽ� ����
        var spawnCmd = new SpawnCommand(
            rec.InstanceID,
            rec.prefabPath,
            transform.position - GameManager.Instance.player.transform.position,
            transform.rotation
        );
        invoker.Record(spawnCmd);

        // 3) �� �ڽ��� �� ���ϱ�
        Vector3 launchForce = forceDirection.normalized * forceMagnitude;
        var rb = this.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(launchForce, ForceMode.Impulse);
        //var throwCmd = new ThrowCommand(rec.InstanceID, launchForce);
        //invoker.RecordAndExecute(throwCmd);
    }
}
