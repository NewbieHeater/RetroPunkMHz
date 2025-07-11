using UnityEngine;

public class Box : MonoBehaviour, IAttackable
{
    [Header("��ȭ/����� Invoker")]
    public Invoker invoker;

    [Header("������")]
    public GameObject boxPrefab;

    [Header("�߻� ����")]
    public float forceMagnitude = 5f;
    public Vector3 forceDirection = Vector3.up + Vector3.right;

    // �� �޼���� �÷��̾��� ���� �ݹ鿡�� ȣ��ȴٰ� ����
    public void OnHitByPlayer()
    {
        
    }

    public void TakeDamage(in DamageInfo info)
    {
        Debug.Log("attakc");
        // 1) ��ɰ�ü ����
        var cmd = new ThrowCommand(
            boxPrefab: boxPrefab,
            spawnPosition: transform.position,
            spawnRotation: transform.rotation,
            launchForce: forceDirection.normalized * forceMagnitude,
            forceMode: ForceMode.Impulse
        );

        // 2) Invoker �� ���� �� ��ȭ ��û
        invoker.ExecuteCommand(cmd);

        // 3) ���� ���ڴ� ��ȭ������ �ÿ��� �ı�
        if (!invoker.isReplaying)  // (IsReplaying ������Ƽ�� Invoker �� ������ �μ���)
            Destroy(gameObject);
    }
}