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

    public GameObject Spawn(string path, Transform parent = null, int warmCount = 1)
    {
        // 풀에서 꺼내기
        GameObject go = ObjectPooler.SpawnFromPool(path, Vector3.zero, parent ?? null);
        if (go == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }
        // UI는 보통 로컬 트랜스폼 초기화가 필요
        var rt = go.transform as RectTransform;
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }
        else
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }

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