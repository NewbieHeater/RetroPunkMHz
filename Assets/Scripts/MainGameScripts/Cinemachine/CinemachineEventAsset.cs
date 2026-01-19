using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Sequence")]
public class CinemachineEventAsset : ScriptableObject
{
    public bool lockPlayerInputWhileRunning = true;
    public float postDelay = 0f;

    // ScriptableObject 참조 리스트 → 제거
    // public List<EventStep> steps = new();

    // 대신 Serializable Class 리스트
    public List<EventStep> steps = new();
}
