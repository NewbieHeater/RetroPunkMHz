using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    void SaveData(SaveData data);
    void LoadData(SaveData data);
}
