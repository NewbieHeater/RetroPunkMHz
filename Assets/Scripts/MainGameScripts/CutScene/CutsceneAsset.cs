using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCutscene", menuName = "Cutscene/Cutscene Asset")]
public class CutsceneAsset : ScriptableObject
{
    public List<CutsceneStep> steps = new List<CutsceneStep>();
}

[System.Serializable]
public class CutsceneStep
{
    public string stepName;
    public Color backgroundColor = Color.black;  // 배경색
    public List<CutsceneFrame> frames = new List<CutsceneFrame>(); // 프레임 리스트
}

[System.Serializable]
public class CutsceneFrame
{
    public string frameName;
    public bool overrideBackground = false;
    public Color backgroundColor = Color.black; // 프레임별 배경 덮어쓰기 가능

    public Sprite centerSprite;   // 중앙 이미지
    public Vector2 centerSize = new Vector2(640, 360);

    [TextArea(2, 4)]
    public List<string> explanations = new List<string>(); // 여러 문장
    public bool useTyping = true;
    public float typingDelay = 0.02f;

    public bool allowSkipTyping = true; // 클릭 시 타이핑 완료
    public bool allowSkipFrame = true;  // 프레임 스킵 가능
}
