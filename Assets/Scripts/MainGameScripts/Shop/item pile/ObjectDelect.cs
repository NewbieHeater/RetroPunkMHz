using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectDelect : MonoBehaviour
{
    [SerializeField] private Item returnItem;
    bool isMouseOver = false;
    void OnMouseEnter() => isMouseOver = true;
    void OnMouseExit() => isMouseOver = false;
    public void SetReturnItem(Item item)
    {
        returnItem = item;
    }
    private void Update()
    {
        if (isMouseOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                itmedelect();
            }


        }

    }
    void itmedelect()
    {

        InventoryManager.Instance.AddItem(returnItem);
        Destroy(gameObject);
    }


}
