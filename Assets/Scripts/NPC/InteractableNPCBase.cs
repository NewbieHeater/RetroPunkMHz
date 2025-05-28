using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public abstract class InteractableNPCBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected GameObject interactionHintUI;  // NPC 상호작용 힌트 (아이콘/텍스트)
    public bool isPlayerInRange = false;

    // 각 NPC마다 다른 동작을 위해 추상 메서드로 둡니다.
    public abstract void Interact();

    private void Awake()
    {
        //interactionHintUI.SetActive(false);
        //interactionHintUI.SetActive(false);
    }

    protected virtual void Start()
    {
        
    }

    // 기본 상호작용 힌트 문구 (필요시 파생 클래스에서 override 가능)
    public virtual string GetInteractPrompt()
    {
        return $"[E] {gameObject.name} 상호작용";
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // 힌트 UI 표시
            if (interactionHintUI != null)
                interactionHintUI.SetActive(true);
            interactionHintUI.GetComponentInChildren<TextMeshProUGUI>().text = GetInteractPrompt();
            // 플레이어에게 자신을 현재 대상로 설정하도록 알림

        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // 힌트 UI 숨기기
            if (interactionHintUI != null)
                interactionHintUI.SetActive(false);
            // 플레이어 현재 상호작용 대상 해제

        }
    }

    protected void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }
}
