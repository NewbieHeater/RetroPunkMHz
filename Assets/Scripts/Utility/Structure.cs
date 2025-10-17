using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PatrolPoint
{
    [Tooltip("순찰할 위치")]
    public Vector3 relativeMovePoint;

    [Tooltip("이 위치에서 기다릴 시간(초)")]
    public float dwellTime;

    [Tooltip("이 위치로 이동할 때 점프가 필요한가?")]
    public bool needJump;

    [Tooltip("점프 높이 (needJump == true 일 때만)")]
    public float jumpPower;
}