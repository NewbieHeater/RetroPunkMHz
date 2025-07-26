using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 슬롯 하나를 담당
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Item mItem; //현재 아이템 인스턴스
    public Item Item
    {
        get
        {
            return mItem;
        }
    }

    [Header("해당 슬롯에 어떠한 타입만 들어올 수 있는지 타입 마스크")]
    [SerializeField] private ItemType mSlotMask;

    private int mItemCount; //획득한 아이템의 개수


    [Header("아이템 슬롯에 있는 UI 오브젝트")]
    [SerializeField] private Image mItemImage; //아이템의 이미지
    [SerializeField] private Image mCooltimeImage; //아이템 쿨타임 이미지
    [SerializeField] private Text mTextCount; //아이템의 개수 텍스트

    [SerializeField] private ItemActionManager mItemActionManager;
    [SerializeField] private ItemDataManager mItemDataManager;
    public ItemDescription mItemDescription;

    // 아이템 이미지의 투명도 조절
    private void SetColor(float _alpha)
    {
        Color color = mItemImage.color;
        color.a = _alpha;
        mItemImage.color = color;
    }

    /// <summary>
    /// mSlotMask에서 설정된 값에 따라 비트연산을한다.
    /// 현재 마스크값이 비트연산으로 0이 나온다면 현재 슬롯에 마스크가 일치하지 않는다는 뜻.
    /// 0이 아닌 수는 현재 비트위치(10진수로 1, 2, 4, 8)로 값이 나온다.
    /// </summary>
    public bool IsMask(Item item)
    {
        return ((int)item.Type & (int)mSlotMask) == 0 ? false : true;
    }

    // 인벤토리에 새로운 아이템 슬롯 추가
    public void AddItem(Item item, int count = 1)
    {
        mItem = item;
        mItemCount = count;
        mItemImage.sprite = mItem.Image;

        if (mItem.Type <= ItemType.Equipment_SHOES)
        {
            mTextCount.text = "";
        }
        else
        {
            mTextCount.text = mItemCount.ToString();
        }

        SetColor(1);
    }

    // 해당 슬롯의 아이템 개수 업데이트
    public void UpdateSlotCount(int count)
    {
        mItemCount += count;
        mTextCount.text = mItemCount.ToString();

        if (mItemCount <= 0)
            ClearSlot();
    }

    // 해당 슬롯 하나 삭제
    public void ClearSlot()
    {
        mItem = null;
        mItemCount = 0;
        mItemImage.sprite = null;
        SetColor(0);

        mTextCount.text = "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mItem != null)
        {
            //쉬프트 (반으로 나누기 모드 활성화)
            if (Input.GetKey(KeyCode.LeftShift)) { DragSlot.Instance.IsShiftMode = true; }
            else { DragSlot.Instance.IsShiftMode = false; }

            //현재 슬롯으로 등록
            DragSlot.Instance.CurrentSlot = this;
            DragSlot.Instance.DragSetImage(mItemImage);
            DragSlot.Instance.transform.position = eventData.position;
        }
    }

    // 마우스 드래그 중 매 프레임마다 호출되는 오버라이드
    public void OnDrag(PointerEventData eventData)
    {
        if (mItem != null)
            DragSlot.Instance.transform.position = eventData.position;
    }

    // 마우스 드래그 종료 오버라이드
    public void OnEndDrag(PointerEventData eventData)
    {
        DragSlot.Instance.AlphaZero();
        DragSlot.Instance.CurrentSlot = null;
    }

    // 해당 슬롯에 무언가가 마우스 드롭 됐을 때 발생하는 이벤트
    public void OnDrop(PointerEventData eventData)
    {
        //쉬프트 모드인 상황에서 해당 위치에 아이템이 있는경우, 반으로 나눌 수 없기에 리턴한다.
        if (DragSlot.Instance.IsShiftMode && mItem != null) { return; }

        //드래그 슬롯에 놓여진 아이템과, 바꿔질 아이템의 마스크가 모두 통과되면 바꾼다.
        if (!IsMask(DragSlot.Instance.CurrentSlot.Item)) { return; }

        //타겟 드래그 슬롯에 이미 아이템이 있는경우, 해당 아이템이 직전의 아이템 슬롯에서 마스크를 체크한다.
        if (mItem != null && !DragSlot.Instance.CurrentSlot.IsMask(mItem)) { return; }

        ChangeSlot();

        //mItemActionManager.SlotOnDropEvent(this); 드롭 이벤트 호출
    }
    private void ChangeSlot()
    {
        if (DragSlot.Instance.CurrentSlot.Item.Type >= ItemType.Etc)
        {
            //쉬프트 모드 관계 없이 일어날 수 있는 경우
            //새로운 슬롯과 이전 슬롯의 아이템ID가 같은경우 개수를 합친다.
            if (mItem != null && mItem.ItemID == DragSlot.Instance.CurrentSlot.Item.ItemID)
            {
                int changedSlotCount;
                if (DragSlot.Instance.IsShiftMode) { changedSlotCount = (int)(DragSlot.Instance.CurrentSlot.mItemCount * 0.5f); }
                else { changedSlotCount = DragSlot.Instance.CurrentSlot.mItemCount; }

                UpdateSlotCount(changedSlotCount);
                DragSlot.Instance.CurrentSlot.UpdateSlotCount(-changedSlotCount);
                return;
            }

            //쉬프트 모드인경우 개수를 반으로 나눈다.
            if (DragSlot.Instance.IsShiftMode)
            {
                //changedSlotCount 개수만큼 더하고 뺄것이다 (+와 -의 차이가 0이어야 아이템이 복사, 유실되지 않는다.)
                int changedSlotCount = (int)(DragSlot.Instance.CurrentSlot.mItemCount * 0.5f);

                //(int)로의 형변환으로 인해 0이 되는 경우는 (int)(1 * 0.5f)이기에 단순히 새로운 슬롯으로 옮긴다.
                if (changedSlotCount == 0)
                {
                    AddItem(DragSlot.Instance.CurrentSlot.Item, 1);
                    DragSlot.Instance.CurrentSlot.ClearSlot();
                    return;
                }

                //위 모든 경우가 아닌경우 새로운 슬롯에 새로운 아이템을 생성한다.
                AddItem(DragSlot.Instance.CurrentSlot.Item, changedSlotCount);
                DragSlot.Instance.CurrentSlot.UpdateSlotCount(-changedSlotCount);
                return;
            }
        }

        //슬롯 서로 교환하기
        Item tempItem = mItem;
        int tempItemCount = mItemCount;

        AddItem(DragSlot.Instance.CurrentSlot.mItem, DragSlot.Instance.CurrentSlot.mItemCount);

        if (tempItem != null) { DragSlot.Instance.CurrentSlot.AddItem(tempItem, tempItemCount); }
        else { DragSlot.Instance.CurrentSlot.ClearSlot(); }
    }

    public void UseItem()
    {
        if (mItem != null) //해당 슬롯의 아이템이 null이라면 return
        {
            //상호작용이 불가능한 (사용이 불가능한) 아이템이라면 리턴
            if (!mItem.IsInteractivity) { return; }

            //쿨타임이 0보다 큰경우 (현재 쿨타임이 돌고있는경우)라면 리턴한다.
            //if (ItemCooltimeManager.Instance.GetCurrentCooltime(mItem.ItemID) > 0) { return; }

            //아이템 사용 함수 호출
            //만약 아이템 함수 호출인 상태에서 false가 리턴되면, 현재 사용 불가능 상태이기에 리턴한다.
            if (!mItemActionManager.UseItem(mItem)) { return; }

            //아이템의 쿨타임이 설정되어있으면 쿨타임 적용
            //if (mItem.Cooltime > 0f) { ItemCooltimeManager.Instance.AddCooltimeQueue(mItem.ItemID, mItem.Cooltime); }

            //상호작용이 가능한(착용 가능한) 장비 아이템을 사용한경우?
            //if (mItem.Type >= ItemType.Equipment_HELMET && mItem.Type <= ItemType.Equipment_SHOES) { ChangeEquipmentSlot(); }

            //아이템이 소모성이면 한개씩 개수를 줄임
            if (mItem != null && mItem.IsConsumable) { UpdateSlotCount(-1); }

            //아이템을 다쓴경우, UpdateSlotCount로 인해 mItem이 null이 되는 경우에 UI를 끈다.
            if (mItem == null) { mItemDescription.CloseUI(); }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)  //버튼 우클릭시
        {
            if (mSlotMask == ItemType.SKILL) { return; }

            UseItem();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mItem != null)
        {


            mItemDescription.OpenUI(mItem.name, mItem.Description);
            mIsTooltipActive = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (mItem != null)
        {
            mItemDescription.CloseUI();
            mIsTooltipActive = false;
        }
    }
    private bool mIsTooltipActive;
    private void Update()
    {
        if (mIsTooltipActive)
        {
            //mToolTipScript.EnableToolTip(mItem.ItemID);
            mIsTooltipActive = false;
        }
    }
}