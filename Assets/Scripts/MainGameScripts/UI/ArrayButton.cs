using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrayButton : MonoBehaviour
{
    [SerializeField] private InventoryMain mInventory;
    [SerializeField] private Button _arraybutton;
     
    private void Start()
    {
        _arraybutton.onClick.AddListener(ArraysButton);
    }

    private void ArraysButton()
    {
        mInventory.InventoryArray();
    }
}
