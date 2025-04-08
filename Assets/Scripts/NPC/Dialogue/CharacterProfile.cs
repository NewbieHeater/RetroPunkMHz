using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Game/Character Profile")]
public class CharacterProfile : ScriptableObject
{
    public string id;                   // 캐릭터 구분 ID (예: "Guard", "Player")
    public string displayName;          // UI에 표시될 이름 (예: "경비병", "플레이어")
    public List<ExpressionSprite> expressions;  // 다양한 표정의 스프라이트 목록

    // 주어진 키에 해당하는 스프라이트 반환 (키가 없으면 첫 번째 스프라이트 반환)
    public Sprite GetSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            // 키가 없으면 기본 표정 (인덱스 0) 반환
            return expressions.Count > 0 ? expressions[0].sprite : null;
        }
        // expressions 리스트에서 키와 일치하는 스프라이트 찾기
        foreach (var expr in expressions)
        {
            if (expr.key == key)
                return expr.sprite;
        }
        // 키를 찾지 못한 경우 첫 번째 스프라이트 반환
        return expressions.Count > 0 ? expressions[0].sprite : null;
    }
}

// 캐릭터 한 표정 이미지와 키를 묶어 표현
[System.Serializable]
public class ExpressionSprite
{
    public string key;     // 표정 키 (예: "neutral", "happy", "angry")
    public Sprite sprite;  // 해당 표정의 이미지
}
