using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// 풀에 tag가 존재하면 풀에서 꺼내고, 아니면 Resources에서 로드 후 Instantiate로 생성한다.
    /// - tag: 풀 키(권장: 프리팹 식별자)
    /// - prefabPathUnderPrefabs: "UI/Popup/MyPopup" 같은 경로 (실제 로드는 "Prefabs/{...}")
    /// </summary>
    public GameObject InstantiateSmart(string tag, string prefabPathUnderPrefabs, Transform parent = null, int warmCountIfCreatePool = 0)
    {
        // 1) 풀에서 시도
        if (ObjectPooler.IsReady && ObjectPooler.TrySpawnFromPool(tag, Vector3.zero, parent, Quaternion.identity, out var pooled))
        {
            PostSpawnTransformFix(pooled);
            return pooled;
        }

        // 2) 풀에 없으면 일반 Instantiate 폴백
        GameObject prefab = Load<GameObject>($"Prefabs/{prefabPathUnderPrefabs}");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab : Prefabs/{prefabPathUnderPrefabs}");
            return null;
        }

        // 3) (선택) 다음부터 풀 쓰고 싶으면 풀 생성 + warm
        if (warmCountIfCreatePool > 0 && ObjectPooler.IsReady)
        {
            // 풀이 없다면 생성
            if (!ObjectPooler.HasPool(tag))
                ObjectPooler.EnsurePool(tag, prefab, warmCountIfCreatePool);
        }

        var go = Object.Instantiate(prefab, parent);
        StripCloneName(go);
        PostSpawnTransformFix(go);
        return go;
    }

    private void StripCloneName(GameObject go)
    {
        int index = go.name.IndexOf("(Clone)");
        if (index > 0) go.name = go.name.Substring(0, index);
    }

    private void PostSpawnTransformFix(GameObject go)
    {
        if (go == null) return;

        // UI면 local, 월드면 localPosition으로 0 맞추는 기존 정책 유지
        if (go.transform is RectTransform rt)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }
        else
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }
    }

    public void Disable(GameObject go)
    {
        if (go == null)
            return;

        Poolable poolable = go.GetComponent<Poolable>();

        if (poolable != null)
        {
            ObjectPooler.ReturnToPool(go);
            return;
        }

        Object.Destroy(go);
    }
    
    public void Destroy(GameObject go)
    {
        Object.Destroy(go);
    }
}