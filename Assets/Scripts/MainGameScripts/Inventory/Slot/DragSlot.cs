using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템을 드래그 할 경우 임시로 DragSlot에 아이템을 저장한다.
/// </summary>
public class DragSlot : Singleton<DragSlot>
{
    public InventorySlot CurrentSlot;
    [HideInInspector] public bool IsShiftMode;
    [SerializeField] private Image mItemImage;

    public void DragSetImage(Image _itemImage)
    {
        mItemImage.sprite = _itemImage.sprite;
        SetColor(1);
    }
    public void AlphaZero()
    {
        SetColor(0);
    }

    public void SetColor(float alpha)
    {
        Color color = mItemImage.color;
        color.a = alpha;
        mItemImage.color = color;
    }
}