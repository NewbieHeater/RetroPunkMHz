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
            //if (interactionHintUI != null)
            //    interactionHintUI.SetActive(true);
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
            //if (interactionHintUI != null)
            //    interactionHintUI.SetActive(false);
            // �÷��̾� ���� ��ȣ�ۿ� ��� ����

        }
    }

    protected void Update()
    {
        bool inRange = isPlayerInRange;
        bool isTalking = DialogueManager.Instance.IsDialogueActive;

        if (inRange && !isTalking)
        {
            // ���� �ȡ���ȭ �� �ƴ� �� ��Ʈ ���̱�
            if (interactionHintUI != null && !interactionHintUI.activeSelf)
                interactionHintUI.SetActive(true);

            // Ű �Է� �� ��ȭ ����
            if (Input.GetKeyDown(KeyCode.F))
            {
                interactionHintUI.SetActive(false);
                Interact();
            }
        }
        else
        {
            // ��ȭ ���̰ų� ���� ��� �� ��Ʈ �����
            if (interactionHintUI != null && interactionHintUI.activeSelf)
                interactionHintUI.SetActive(false);
        }
    }
}
