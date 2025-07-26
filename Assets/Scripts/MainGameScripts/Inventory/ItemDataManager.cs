using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataManager : Singleton<ItemDataManager>
{
    private string Name;
    private string Description;

    public string GetName(int id)
    {
        return Name;
    }

    public string GetDescription(int id)
    {
        return Description;
    }
}
