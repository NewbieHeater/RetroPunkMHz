using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDelect : MonoBehaviour
{
    [SerializeField] private Item returnItem;

    public void SetReturnItem(Item item)
    {
        returnItem = item;

    }

    void OnMouseDown()
    {
        Destroy(gameObject);
        InventoryMain.Instance.AcquireItem(returnItem);
    }
}
