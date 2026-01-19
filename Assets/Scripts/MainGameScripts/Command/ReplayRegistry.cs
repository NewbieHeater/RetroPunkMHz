using System.Collections.Generic;
using UnityEngine;

public static class ReplayRegistry
{
    static Dictionary<string, GameObject> instances = new();
    public static void RegisterInstance(string id, GameObject obj)
        => instances[id] = obj;
    public static bool TryGetInstance(string id, out GameObject obj)
        => instances.TryGetValue(id, out obj);
    public static void Clear() => instances.Clear();
}
