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

    }

    protected virtual void Start()
    {
        
    }

    // �⺻ ��ȣ�ۿ� ��Ʈ ���� (�ʿ�� �Ļ� Ŭ�������� override ����)
    public virtual string GetInteractPrompt()
    {
        return $"[E] {gameObject.name} ��ȣ�ۿ�";
    }
}
