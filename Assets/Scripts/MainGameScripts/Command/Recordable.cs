using UnityEngine;

[DisallowMultipleComponent]
public class Recordable : MonoBehaviour
{
    [Tooltip("Resources ���� ���� ���(ex: Prefabs/Box)")]
    public string prefabPath;

    // ��ȭ/��� �� �ν��Ͻ� �ĺ��� ID
    public string InstanceID { get; set; }

    void Awake()
    {
        // Play ��帶�� ���ο� ID ����
        InstanceID = System.Guid.NewGuid().ToString();
    }
}
