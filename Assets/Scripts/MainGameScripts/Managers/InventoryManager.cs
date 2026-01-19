using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField, Range(8, 64)]
    public int _maxCapacity = 45;

    [SerializeField]
    private Item[] _items;

    public event Action<int> OnItemChanged;

    protected override void Awake()
    {
        base.Awake();
        _items = new Item[_maxCapacity];
        for (int i = 0; i < _maxCapacity; i++)
            _items[i] = null;

    }

    public Item GetItem(int index)
    {
        if (index < 0 || index >= _items.Length) return null;
        return _items[index];
    }

    public void SetItem(int index, Item item)
    {
        if (index < 0 || index >= _items.Length) return;

        _items[index] = item;

        OnItemChanged?.Invoke(index);
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null)
            {
                _items[i] = item;
                OnItemChanged?.Invoke(i);
                return true;
            }
        }

        // ÀÎº¥Åä¸® ²Ë Âò
        return false;
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= _items.Length) return;

        _items[index] = null;
        OnItemChanged?.Invoke(index);
    }

    public void SwapItem(int indexA, int indexB)
    {
        if (indexA == indexB) return;
        if (indexA < 0 || indexA >= _items.Length) return;
        if (indexB < 0 || indexB >= _items.Length) return;

        Item temp = _items[indexA];
        _items[indexA] = _items[indexB];
        _items[indexB] = temp;

        OnItemChanged?.Invoke(indexA);
        OnItemChanged?.Invoke(indexB);
    }
}
