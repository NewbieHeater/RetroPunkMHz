using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }
    public T[] LoadAll<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(path);
    }

    public GameObject[] LoadPrefab(string path)
    {
        GameObject[] prefabs = LoadAll<GameObject>($"Prefabs/{path}");



        return prefabs;
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }
        GameObject go = Object.Instantiate(prefab, parent);
        int index = go.name.IndexOf("(Clone)");
        if (index > 0)
            go.name = go.name.Substring(0, index);

        return go;
    }

    public void Disable(GameObject go)
    {
        if (go == null)
            return;
        go.SetActive(false);
        ObjectPooler.ReturnToPool(go);
    }
    public void Destroy(GameObject go)
    {
        Object.Destroy(go);
    }
}