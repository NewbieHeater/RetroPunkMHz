using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PrefabPalette", menuName = "LevelEditing/Prefab Palette", order = 10)]
public class PrefabPalette : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string label;               // UI 라벨(선택)
        public string tag;                 // 카테고리/태그(필터용)
        public GameObject prefab;          // 배치할 프리팹
        public Texture2D customIcon;       // 썸네일 덮어쓰기(선택)
        public bool favorite;              // 즐겨찾기
    }

    public List<Entry> items = new List<Entry>();
}