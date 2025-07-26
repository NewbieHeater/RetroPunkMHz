using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public TextMeshProUGUI itemname;
    public TextMeshProUGUI itemcount;
    public Button buybutton;

    private int item = 10;
    private void Start()
    {
        itemname.text = "potion";
        UpdatecountText();

        buybutton.onClick.AddListener(Onbuybutton);
    }

    private void Onbuybutton()
    {
        if(item >0)
        {
            item--;
            UpdatecountText();
        }
        else
        {
            Debug.Log("close");
        }
    }

    private void UpdatecountText()
    {
        itemcount.text = item+"gae";
    }
}
