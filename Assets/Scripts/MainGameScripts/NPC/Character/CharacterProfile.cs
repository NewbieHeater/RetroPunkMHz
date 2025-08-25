using UnityEngine;
using System.Collections.Generic;

public enum CharacterSide { Left, Right }

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Game/Character Profile")]
public class CharacterProfile : ScriptableObject
{
    [Header("Identity")]
    public string id;                 // "Guard", "Eto", "Merchant" …
    public string displayName;        // UI 표시명

    [Header("UI Defaults")]
    public CharacterSide defaultSide = CharacterSide.Right; // 플레이어는 Left 권장
    public string defaultExpressionKey = "neutral";
    public Sprite defaultExpressionSprite;                   // 비워두면 key로 폴백

    [Header("Expressions")]
    public List<ExpressionSprite> expressions = new();

    [System.NonSerialized] private Dictionary<string, Sprite> _exprMap;

    void OnEnable()
    {
        _exprMap = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var e in expressions)
            if (!string.IsNullOrWhiteSpace(e.key)) _exprMap[e.key.Trim()] = e.sprite;

        // 흔한 오타 보정
        if (_exprMap.ContainsKey("neutral") && !_exprMap.ContainsKey("netural"))
            _exprMap["netural"] = _exprMap["neutral"];
    }

    public Sprite GetSprite(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return GetDefaultSprite();

        var norm = Normalize(key);
        if (_exprMap.TryGetValue(norm, out var sp)) return sp;

        // 오타 보정
        if (norm == "netural" && _exprMap.TryGetValue("neutral", out sp)) return sp;

        return GetDefaultSprite();
    }

    public bool TryGetSprite(string key, out Sprite sprite)
    {
        sprite = GetSprite(key);
        return sprite != null;
    }

    public bool HasExpression(string key)
    {
        return _exprMap.ContainsKey(Normalize(key));
    }

    public Sprite GetDefaultSprite()
    {
        if (defaultExpressionSprite != null) return defaultExpressionSprite;
        if (_exprMap != null && _exprMap.TryGetValue(Normalize(defaultExpressionKey), out var sp)) return sp;
        if (expressions != null && expressions.Count > 0) return expressions[0].sprite;
        return null;
    }

    private static string Normalize(string s) => s?.Trim().ToLowerInvariant();
}

[System.Serializable]
public class ExpressionSprite
{
    public string key;     // "neutral", "happy", "angry" …
    public Sprite sprite;
}
