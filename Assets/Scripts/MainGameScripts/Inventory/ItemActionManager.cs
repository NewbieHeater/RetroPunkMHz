using Unity.VisualScripting;
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


    [Header("Preloaded objects into the scene")]
    [SerializeField] private GameObject[] _objects;
    
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
                    switch (item.ItemID)
                    {
                        case (int)ItemCode.Communicator:
                        {
                                if(SceneManagerEx.Instance.isGameSceneActive == false)
                                {
                                    Debug.Log("후후");
                                    SceneManagerEx.Instance.LoadScene(Define.Scene.RoomScene);
                                    SceneManagerEx.Instance.isGameSceneActive = true;
                                    
                                }
                                else if(SceneManagerEx.Instance.isGameSceneActive == true)
                                {
                                    Debug.Log("호호");
                                    SceneManagerEx.Instance.LoadScene(Define.Scene.GameScene);
                                    SceneManagerEx.Instance.isGameSceneActive = false;
                                }
                                break;
                        }
                    }
                }
                break;
            case ItemType.Placeable://Placeable(설치가능한)으로 만들어두고 
                {
                    Debug.Log("두둥");
                    // 아마 이부분은 getcomponent말고 다른거 써야하겠죠?
                    Uiclickset _clickset = GetComponent<Uiclickset>();
                    _clickset.HandleInventoryClick(item.itemPrefab);
                    // 지금은 다른씬에서 사용하는 Uiclickset에 일일히 오브젝트를 넣어서 해당 오브젝트를 배치시키고있습니다
                    // 새로운 아이템 넣을떄마다 HandleInventoryClick1, 2, 3... 로 함수가 많아지면 안되겠죠?
                    // 제가 아이템 스크립터블 오브젝트의 분류에 PlaceableItem으로 만들어둘게요 해당 아이템들은 추가적으로 프리팹을 가질수있게 할겁니다
                    // 기존의 아이템을 삭제후 다시 PlaceableItem으로 만드세요 Communicator 제외하고요
                    // create -> AddItem -> PlaceableItem
                    // 그러면 
                    // _clickset.HandleInventoryClick(item.itemPrefab); 으로 설치시킬수있겠죠?
                    // 그러면Uiclickset에서 HandleInventoryClick(GameObject itemPrefab) 함수에 매개변수를 넣어줘야 할겁니다


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
    NULL,
    Communicator,
    PC,
    TurnTable,
}