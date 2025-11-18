using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Add Item/Item/PlaceableItem")]
public class PlaceableItem : Item
{

    [Header("배치시 사용할 프리팹")]
    [SerializeField] private GameObject mItemPrefab;
    /// <summary>
    /// 아이템이 중첩이 가능한가?
    /// </summary>
    /// <value></value>
    public GameObject itemPrefab
    {
        get
        {
            return mItemPrefab;
        }
    }
}
