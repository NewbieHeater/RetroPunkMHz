using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Sequence")]
public class CinemachineEventAsset : ScriptableObject
{
    public bool lockPlayerInputWhileRunning = true;
    public float postDelay = 0f;
    public List<EventStep> steps = new();
}
