using UnityEngine;


/// <summary>
/// �� ���� �Ŵ��� ������Ʈ�� �Ҵ�
/// ������(�Ǵ� ���� ��ü)�� ��ȣ�ۿ��ϰų�, �κ��丮���� �������� ����ϸ� Ư�� �̺�Ʈ�� �߻���Ŵ
/// </summary>
public class ItemActionManager : MonoBehaviour
{
    /// <summary>
    /// �޽����� �ְ�޴°�� ��ų�� ���� �޽��� ���
    /// </summary>
    public static string _SkillMessage = "ActiveSkill";

    [SerializeField] private RigidPlayerManagement mPlayerController;

    [Header("Preloaded objects into the scene")]
    [SerializeField] private GameObject[] mObjects;

    /// <summary>
    /// ������ ��� �̺�Ʈ ȣ��
    /// �� �����۸��� ����Ǵ� ����� ����
    /// </summary>
    /// <param name="item"></param>
    /// <returns>������ ���������� �̷�� ���°�?</returns>
    public bool UseItem(Item item)
    {
        Debug.Log("UseItemEvent");

        switch (item.Type)
        {
            case ItemType.SKILL:
                {

                }
                break;
            case ItemType.Consumable:
                {
                    switch (item.ItemID)
                    {
                        case (int)ItemCode.SMALL_HEALTH_POTION:
                            {
                                //GameManager.Instance.Player.ModifyHP(50);
                                //SoundManager.Instance.PlaySound2D("Food Drink " + SoundManager.Range(1, 4, true));
                                break;
                            }
                        case (int)ItemCode.SMALL_MANA_POTION:
                            {
                                //GameManager.Instance.Player.ModifyMana(50);
                                //SoundManager.Instance.PlaySound2D("Food Drink " + SoundManager.Range(1, 4, true));
                                break;
                            }
                    }

                    break;
                }
        }

        return true;
    }

    /// <summary>
    /// �� ������ �������� �ݰų�, NONEŸ��(���� �ʰ�, ��ȣ�ۿ� ����) �����۰� ��ȣ�ۿ��Ѱ�� ����Ǵ� �Լ�
    /// </summary>
    /// <param name="itemID">�ش� �������� �ڵ�</param>
    /// <param name="interactTarget"></param>
    public void InteractionItem(Item item, GameObject interactTarget)
    {
        Debug.Log("InteractionItemEvent");

        if (interactTarget.tag == "NPC")
        {
            //NPC FSM ��������
            //NPCBase targetNPC = interactTarget.GetComponent<NPCBase>();

            //���� ��ȣ�ۿ��� �Ұ����� ����̶�� ����
            //if (!targetNPC.CanInteraction || targetNPC.IsQuotePlaying) { return; }

            //��ȣ�ۿ� �޽��� ����
            //MessageDispatcher.Instance.DispatchMessage(0, "", targetNPC.EntityName, "Interaction");
            return;
        }
    }

    /// <summary>
    /// �������� ���Կ� ����ϴ°�� �߻��ϴ� �̺�Ʈ�̴�.
    /// </summary>
    /// <param name="slot">��ӵ� ����</param>
    public void SlotOnDropEvent(InventorySlot slot)
    {
        Debug.Log("SlotOnDropEvent");
    }
}

public enum ItemCode
{
    SMALL_HEALTH_POTION,
    SMALL_MANA_POTION,
}