using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowHp : Singleton<ShowHp>
{
    float hp = 1000;
    public TextMeshProUGUI TextMeshProUGUI;
    void Start()
    {
        
    }

    
    void Update()
    {
        TextMeshProUGUI.text = $"{hp}";
    }

    public void MinusTMP(float num)
    {
        hp -= num ; 
    }
}
