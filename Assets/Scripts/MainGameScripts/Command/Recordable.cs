using UnityEngine;

[DisallowMultipleComponent]
public class Recordable : MonoBehaviour
{
    [Tooltip("Resources 폴더 기준 경로(ex: Prefabs/Box)")]
    public string prefabPath;

    // 녹화/재생 시 인스턴스 식별용 ID
    public string InstanceID { get; set; }

    void Awake()
    {
        // Play 모드마다 새로운 ID 생성
        InstanceID = System.Guid.NewGuid().ToString();
    }
}
