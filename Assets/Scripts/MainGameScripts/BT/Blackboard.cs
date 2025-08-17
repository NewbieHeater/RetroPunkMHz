using System.Collections.Generic;

public enum NodeStatus { Success, Failure, Running }

public sealed class Blackboard
{
    private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

    public void Set<T>(string key, T value) => _data[key] = value;
    public bool TryGet<T>(string key, out T value)
    {
        if (_data.TryGetValue(key, out var obj) && obj is T cast)
        {
            value = cast; return true;
        }
        value = default(T);
        return false;
    }
    public T Get<T>(string key, T defaultValue = default(T))
    {
        return TryGet<T>(key, out var v) ? v : defaultValue;
    }
    public bool Has(string key) => _data.ContainsKey(key);
    public bool Remove(string key) => _data.Remove(key);
    public void Clear() => _data.Clear();
}
