using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public abstract class InteractableNPCBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected GameObject interactionHintUI;  // NPC ��ȣ�ۿ� ��Ʈ (������/�ؽ�Ʈ)
    public bool isPlayerInRange = false;

    // �� NPC���� �ٸ� ������ ���� �߻� �޼���� �Ӵϴ�.
    public abstract void Interact();

    private void Awake()
    {
        //interactionHintUI.SetActive(false);
        //interactionHintUI.SetActive(false);
    }

    protected virtual void Start()
    {
        
    }

    // �⺻ ��ȣ�ۿ� ��Ʈ ���� (�ʿ�� �Ļ� Ŭ�������� override ����)
    public virtual string GetInteractPrompt()
    {
        return $"[E] {gameObject.name} ��ȣ�ۿ�";
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // ��Ʈ UI ǥ��
            if (interactionHintUI != null)
                interactionHintUI.SetActive(true);
            interactionHintUI.GetComponentInChildren<TextMeshProUGUI>().text = GetInteractPrompt();
            // �÷��̾�� �ڽ��� ���� ���� �����ϵ��� �˸�

        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // ��Ʈ UI �����
            if (interactionHintUI != null)
                interactionHintUI.SetActive(false);
            // �÷��̾� ���� ��ȣ�ۿ� ��� ����

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
