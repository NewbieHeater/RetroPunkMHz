using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inven : UI_Popup
{
    enum GameObjects
    {
        GridPanel,
        DraggingIcon
    }

    private GraphicRaycaster _gr;
    private PointerEventData _ped;
    private List<RaycastResult> _rrList;

    // 드래그 상태
    private bool _isDragging = false;
    private UI_Inven_Item _draggedSlot;        // 드래그 시작한 슬롯
    private GameObject _draggingIcon;          // 마우스를 따라다니는 아이콘
    private Image _draggingIconImage;

    private List<UI_Inven_Item> _slots = new List<UI_Inven_Item>();

    private const float DRAG_THRESHOLD = 10f;   // 픽셀
    private bool _isPointerDown = false;
    private Vector2 _pointerDownPos;
    private UI_Inven_Item _pointerDownSlot;

    private ItemActionManager _itemActionManager;

    void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= HandleItemChanged;
            InventoryManager.Instance.OnItemChanged += HandleItemChanged;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= HandleItemChanged;
        }
    }    

    public override void Init()
    {
        base.Init();

        TryGetComponent(out _gr);
        if (_gr == null) _gr = gameObject.AddComponent<GraphicRaycaster>();
        _ped = new PointerEventData(EventSystem.current);
        _rrList = new List<RaycastResult>();


        Bind<GameObject>(typeof(GameObjects));

        GameObject gridPanel = Get<GameObject>((int)GameObjects.GridPanel);

        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);

        _draggingIcon = Get<GameObject>((int)GameObjects.DraggingIcon);
        _draggingIconImage = _draggingIcon.GetComponent<Image>();

        _slots.Clear();

        for (int i = 0; i < InventoryManager.Instance._maxCapacity; i++)
        {
            GameObject item = Managers.UI.MakeSubItem<UI_Inven_Item>(gridPanel.transform).gameObject;            
            UI_Inven_Item invenItem = item.GetOrAddComponent<UI_Inven_Item>();
            invenItem.SetIndex(i);
            _slots.Add(invenItem);
            //invenItem.Refresh();
        }

        DraggingIcon();

        if (_itemActionManager == null)
            _itemActionManager = FindObjectOfType<ItemActionManager>();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    #region 드래그 아이콘 생성/이동

    private void DraggingIcon()
    {
        _draggingIconImage.raycastTarget = false; // 이 아이콘은 레이캐스트 대상이 되지 않도록
        

        _draggingIcon.SetActive(false);
    }

    private void UpdateDraggingIconPosition()
    {
        if (_draggingIcon == null) return;

        Vector2 pos = Input.mousePosition;
        _draggingIcon.transform.position = pos;
    }

    #endregion

    #region 마우스 입력 처리

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UI_Inven_Item slot = RaycastAndGetFirstComponent<UI_Inven_Item>();
            if (slot != null)
            {
                Item item = InventoryManager.Instance.GetItem(slot.Index);
                if (item != null)
                {
                    _isPointerDown = true;
                    _pointerDownPos = Input.mousePosition;
                    _pointerDownSlot = slot;

                    // 드래그는 아직 시작하지 않은 상태
                    _isDragging = false;
                    _draggedSlot = null;
                }
            }
        }

        // 드래그 중이면 아이콘을 마우스를 따라다니게
        if (_isPointerDown && !_isDragging && Input.GetMouseButton(0))
        {
            float dist = Vector2.Distance(_pointerDownPos, Input.mousePosition);
            if (dist >= DRAG_THRESHOLD)
            {
                // 드래그 시작
                Item item = InventoryManager.Instance.GetItem(_pointerDownSlot.Index);
                if (item != null)
                {
                    StartDrag(_pointerDownSlot, item);
                }
            }
        }

        // 드래그 중이면 아이콘을 마우스를 따라다니게
        if (_isDragging)
        {
            UpdateDraggingIconPosition();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging)
            {
                // 드래그 종료 → 슬롯 교환
                UI_Inven_Item targetSlot = RaycastAndGetFirstComponent<UI_Inven_Item>();
                EndDrag(targetSlot);
            }
            else if (_isPointerDown && _pointerDownSlot != null)
            {
                // 드래그가 아니면 "클릭"으로 처리 → 아이템 사용
                HandleClickUse(_pointerDownSlot);
            }

            // 상태 초기화
            _isPointerDown = false;
            _pointerDownSlot = null;
        }
    }

    private void StartDrag(UI_Inven_Item slot, Item item)
    {
        _isDragging = true;
        _draggedSlot = slot;

        _draggingIcon.SetActive(true);
        _draggingIconImage.sprite = item.Image;
        //_draggingIconImage.SetNativeSize(); // 아이콘 원본 크기 사용가능

        UpdateDraggingIconPosition();
    }

    private void EndDrag(UI_Inven_Item targetSlot)
    {
        _isDragging = false;
        _draggingIcon.SetActive(false);

        if (targetSlot == null || _draggedSlot == null)
        {
            _draggedSlot = null;
            return;
        }

        int from = _draggedSlot.Index;
        int to = targetSlot.Index;

        if (from == to)
        {
            _draggedSlot = null;
            return;
        }

        InventoryManager.Instance.SwapItem(from, to);

        // 이벤트 시스템을 쓰고 있다면 아래 Refresh는 없어도 되지만,
        // 안전하게 한 번 더 갱신
        _draggedSlot.Refresh();
        targetSlot.Refresh();

        _draggedSlot = null;
    }

    private void HandleClickUse(UI_Inven_Item slot)
    {
        Item item = InventoryManager.Instance.GetItem(slot.Index);
        if (item == null)
            return;

        if (_itemActionManager != null)
        {
            bool used = _itemActionManager.UseItem(item);

            if (item.IsConsumable)
            {
                InventoryManager.Instance.RemoveItem(slot.Index);
            }
        }
        else
        {
            Debug.LogWarning("ItemActionManager를 찾을 수 없습니다.");
        }
    }

    #endregion

    private T RaycastAndGetFirstComponent<T>() where T : Component
    {
        if (_gr == null || _ped == null)
            return null;

        _rrList.Clear();
        _ped.position = Input.mousePosition;
        _gr.Raycast(_ped, _rrList);

        if (_rrList.Count == 0)
            return null;

        // 첫 번째 hit만 보지 말고,
        // 위에서부터 T를 가진 오브젝트를 찾는다.
        foreach (var r in _rrList)
        {
            T comp = r.gameObject.GetComponent<T>();
            if (comp != null)
                return comp;
        }

        return null;
    }

    public void RefreshAll()
    {
        foreach (var slot in _slots)
            slot.Refresh();
    }

    private void HandleItemChanged(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count)
            return;

        _slots[slotIndex].Refresh();
    }
}
