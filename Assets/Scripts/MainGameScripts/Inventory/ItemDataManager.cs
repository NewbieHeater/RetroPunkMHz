using System.Collections.Generic;
using UnityEngine;

public class ItemDataManager : Singleton<ItemDataManager>
{
    [SerializeField] private List<Item> _allItems;
    private Dictionary<int, Item> _itemDict;

    protected override void Initialize()
    {
        if (_allItems != null) return;

        _allItems = new List<Item>();
        _itemDict = new Dictionary<int, Item>();

        var loadedItems = Resources.LoadAll<Item>("Items/");
        foreach (var item in loadedItems)
        {
            _allItems.Add(item);
            _itemDict[item.ItemID] = item;
        }
    }

    public IReadOnlyList<Item> GetAll() => _allItems;
    public Item GetById(int id) => _itemDict.TryGetValue(id, out var item) ? item : null;
}
