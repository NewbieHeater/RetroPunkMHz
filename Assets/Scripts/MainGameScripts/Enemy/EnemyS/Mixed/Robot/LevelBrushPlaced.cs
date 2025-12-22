using UnityEngine;

/// <summary>
/// LevelBrush로 배치된 인스턴스임을 표시하는 마커.
/// (Editor 폴더 밖에 두세요!)
/// </summary>
[DisallowMultipleComponent]
public sealed class LevelBrushPlaced : MonoBehaviour
{
    [Tooltip("false면 Combiner가 이 오브젝트를 무시합니다.")]
    public bool allowCombine = true;

    [Tooltip("false면 브러쉬 지우개/삭제가 이 오브젝트를 건드리지 않습니다.")]
    public bool allowErase = true;
}
