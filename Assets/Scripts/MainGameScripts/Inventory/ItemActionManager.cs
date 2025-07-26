using UnityEngine;


/// <summary>
/// 씬 내의 매니저 오브젝트에 할당
/// 아이템(또는 정적 물체)과 상호작용하거나, 인벤토리에서 아이템을 사용하면 특수 이벤트를 발생시킴
/// </summary>
public class ItemActionManager : MonoBehaviour
{
    /// <summary>
    /// 메시지를 주고받는경우 스킬에 대한 메시지 약속
    /// </summary>
    public static string _SkillMessage = "ActiveSkill";

    [SerializeField] private RigidPlayerManagement mPlayerController;

    [Header("Preloaded objects into the scene")]
    [SerializeField] private GameObject[] mObjects;

    /// <summary>
    /// 아이템 사용 이벤트 호출
    /// 각 아이템마다 실행되는 기능을 수행
    /// </summary>
    /// <param name="item"></param>
    /// <returns>실행이 정상적으로 이루어 졌는가?</returns>
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
    /// 씬 내에서 아이템을 줍거나, NONE타입(줍지 않고, 상호작용 전용) 아이템과 상호작용한경우 실행되는 함수
    /// </summary>
    /// <param name="itemID">해당 아이템의 코드</param>
    /// <param name="interactTarget"></param>
    public void InteractionItem(Item item, GameObject interactTarget)
    {
        Debug.Log("InteractionItemEvent");

        if (interactTarget.tag == "NPC")
        {
            //NPC FSM 가져오기
            //NPCBase targetNPC = interactTarget.GetComponent<NPCBase>();

            //현재 상호작용이 불가능한 대상이라면 리턴
            //if (!targetNPC.CanInteraction || targetNPC.IsQuotePlaying) { return; }

            //상호작용 메시지 보냄
            //MessageDispatcher.Instance.DispatchMessage(0, "", targetNPC.EntityName, "Interaction");
            return;
        }
    }

    /// <summary>
    /// 아이템을 슬롯에 드롭하는경우 발생하는 이벤트이다.
    /// </summary>
    /// <param name="slot">드롭된 슬롯</param>
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