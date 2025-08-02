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
    private Item _item;
    public Item Item => _item;

    [Header("해당 슬롯에 어떠한 타입만 들어올 수 있는지 타입 마스크")]
    [SerializeField] private ItemType _slotMask;

    private int _itemCount;

    [Header("아이템 슬롯에 있는 UI 오브젝트")]
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _cooltimeImage;
    [SerializeField] private Text _textCount;

    [SerializeField] private ItemActionManager _itemActionManager;
    [SerializeField] private ItemDescription _itemDescription;

    private bool _isTooltipActive;

    private void SetColor(float alpha)
    {
        Color color = _itemImage.color;
        color.a = alpha;
        _itemImage.color = color;
    }

    public bool IsMask(Item item)
    {
        return ((int)item.Type & (int)_slotMask) != 0;
    }

    public void AddItem(Item item, int count = 1)
    {
        _item = item;
        _itemCount = count;
        _itemImage.sprite = _item.Image;

        _textCount.text = _item.Type <= ItemType.Equipment_SHOES ? "" : _itemCount.ToString();

        SetColor(1);
    }

    public void UpdateSlotCount(int count)
    {
        _itemCount += count;
        _textCount.text = _itemCount.ToString();

        if (_itemCount <= 0)
            ClearSlot();
    }

    public void ClearSlot()
    {
        _item = null;
        _itemCount = 0;
        _itemImage.sprite = null;
        SetColor(0);
        _textCount.text = "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item != null)
        {
            DragSlot.Instance.IsShiftMode = Input.GetKey(KeyCode.LeftShift);
            DragSlot.Instance.CurrentSlot = this;
            DragSlot.Instance.DragSetImage(_itemImage);
            DragSlot.Instance.transform.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_item != null)
            DragSlot.Instance.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragSlot.Instance.AlphaZero();
        DragSlot.Instance.CurrentSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragSlot.Instance.IsShiftMode && _item != null) return;
        if (!IsMask(DragSlot.Instance.CurrentSlot.Item)) return;
        if (_item != null && !DragSlot.Instance.CurrentSlot.IsMask(_item)) return;

        ChangeSlot();
    }

    private void ChangeSlot()
    {
        Item draggedItem = DragSlot.Instance.CurrentSlot.Item;
        int draggedCount = DragSlot.Instance.CurrentSlot._itemCount;

        if (draggedItem.Type >= ItemType.Etc)
        {
            if (_item != null && _item.ItemID == draggedItem.ItemID)
            {
                int changedCount = DragSlot.Instance.IsShiftMode
                    ? (int)(draggedCount * 0.5f)
                    : draggedCount;

                UpdateSlotCount(changedCount);
                DragSlot.Instance.CurrentSlot.UpdateSlotCount(-changedCount);
                return;
            }

            if (DragSlot.Instance.IsShiftMode)
            {
                int changedCount = (int)(draggedCount * 0.5f);
                if (changedCount == 0)
                {
                    AddItem(draggedItem, 1);
                    DragSlot.Instance.CurrentSlot.ClearSlot();
                    return;
                }

                AddItem(draggedItem, changedCount);
                DragSlot.Instance.CurrentSlot.UpdateSlotCount(-changedCount);
                return;
            }
        }

        Item tempItem = _item;
        int tempCount = _itemCount;

        AddItem(draggedItem, draggedCount);

        if (tempItem != null)
            DragSlot.Instance.CurrentSlot.AddItem(tempItem, tempCount);
        else
            DragSlot.Instance.CurrentSlot.ClearSlot();
    }

    public void UseItem()
    {
        if (_item == null) return;
        if (!_item.IsInteractivity) return;
        // if (ItemCooltimeManager.Instance.GetCurrentCooltime(_item.ItemID) > 0) return;

        if (!_itemActionManager.UseItem(_item)) return;

        // if (_item.Cooltime > 0f) ItemCooltimeManager.Instance.AddCooltimeQueue(_item.ItemID, _item.Cooltime);
        // if (_item.Type >= ItemType.Equipment_HELMET && _item.Type <= ItemType.Equipment_SHOES) ChangeEquipmentSlot();

        if (_item.IsConsumable)
            UpdateSlotCount(-1);

        if (_item == null)
            _itemDescription.CloseUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_slotMask == ItemType.SKILL) return;
            UseItem();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item != null)
        {
            _itemDescription.OpenUI(_item.name, _item.Description);
            _isTooltipActive = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_item != null)
        {
            _itemDescription.CloseUI();
            _isTooltipActive = false;
        }
    }

    private void Update()
    {
        if (_isTooltipActive)
        {
            // mToolTipScript.EnableToolTip(_item.ItemID);
            _isTooltipActive = false;
        }
    }
}
