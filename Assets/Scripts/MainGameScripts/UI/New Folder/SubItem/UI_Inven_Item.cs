using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inven_Item : UI_Base
{
    enum GameObjects
    {
        ItemIcon,
        ItemNum,
    }

    private int _index; // 이 슬롯이 인벤토리에서 몇 번째인지
    private Image _iconImage;
    private TextMeshProUGUI _nameText;

    public int Index => _index;
    public Sprite IconSprite => _iconImage.sprite;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        _iconImage = Get<GameObject>((int)GameObjects.ItemIcon).GetComponent<Image>();
        _nameText = Get<GameObject>((int)GameObjects.ItemNum).GetComponent<TextMeshProUGUI>();
        Refresh();
    }

    
    public void SetIndex(int index)
    {
        _index = index;
    }

    public void Refresh()
    {
        Item item = InventoryManager.Instance.GetItem(_index);

        if (item == null)
        {
            _iconImage.enabled = false;
            _nameText.text = "";
        }
        else
        {
            _iconImage.enabled = true;
            _iconImage.sprite = item.Image;   // 아이콘 추가하면 사용 가능
            _nameText.text = item.name;
        }
    }
}
