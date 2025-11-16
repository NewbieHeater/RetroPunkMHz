using UnityEngine;

public abstract class StoryVariable : ScriptableObject
{
    [SerializeField] private string key;   // 저장용 ID (유니크)
    public string Key => key;
}
