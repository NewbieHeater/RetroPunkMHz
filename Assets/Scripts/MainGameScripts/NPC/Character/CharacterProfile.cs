using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Game/Character Profile")]
public class CharacterProfile : ScriptableObject
{
    public string id;                   // ĳ���� ���� ID (��: "Guard", "Player")
    public string displayName;          // UI�� ǥ�õ� �̸� (��: "���", "�÷��̾�")
    public List<ExpressionSprite> expressions;  // �پ��� ǥ���� ��������Ʈ ���

    // �־��� Ű�� �ش��ϴ� ��������Ʈ ��ȯ (Ű�� ������ ù ��° ��������Ʈ ��ȯ)
    public Sprite GetSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            // Ű�� ������ �⺻ ǥ�� (�ε��� 0) ��ȯ
            return expressions.Count > 0 ? expressions[0].sprite : null;
        }
        // expressions ����Ʈ���� Ű�� ��ġ�ϴ� ��������Ʈ ã��
        foreach (var expr in expressions)
        {
            if (expr.key == key)
                return expr.sprite;
        }
        // Ű�� ã�� ���� ��� ù ��° ��������Ʈ ��ȯ
        return expressions.Count > 0 ? expressions[0].sprite : null;
    }
}

// ĳ���� �� ǥ�� �̹����� Ű�� ���� ǥ��
[System.Serializable]
public class ExpressionSprite
{
    public string key;     // ǥ�� Ű (��: "neutral", "happy", "angry")
    public Sprite sprite;  // �ش� ǥ���� �̹���
}
