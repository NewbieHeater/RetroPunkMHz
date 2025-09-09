using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public TextMeshProUGUI itemname;
    public TextMeshProUGUI itemcount;
    public Button buybutton;

    private int item = 10;
    private int _inventorycount = 0;
    
    
    
    
    private void Start()
    {
        itemname.text = "포션";
        UpdatecountText();

        buybutton.onClick.AddListener(Onbuybutton);
    }
    private void Onbuybutton()
    {
        if(item >0)
        {
            item--;
            _inventorycount = 10 - item;
            UpdatecountText();
            
        }
        else
        {
            Debug.Log("close");
        }
    }

    private void UpdatecountText()
    {
        itemcount.text = item+".";
    }
}
