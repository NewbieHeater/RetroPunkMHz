// Scripts/Story/Events/GameEventSO.cs
using UnityEngine;

public abstract class GameEventSO : ScriptableObject
{
    public abstract void Invoke();
}
