using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public abstract class InteractableNPCBase : MonoBehaviour
{
    [SerializeField] protected GameObject interactionHintUI;  // NPC 상호작용 힌트 (아이콘/텍스트)
    [SerializeField] private float range = 2.0f;
    public bool isPlayerInRange = false;

    public Transform Xform => transform;

    public float Range => range;

    // 각 NPC마다 다른 동작을 위해 추상 메서드로 둡니다.
    public abstract void Interact();

    private void Awake()
    {

    }

    protected virtual void Start()
    {
        
    }

    // 기본 상호작용 힌트 문구 (필요시 파생 클래스에서 override 가능)
    public virtual string GetInteractPrompt()
    {
        return $"[E] {gameObject.name} 상호작용";
    }
}
