using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Simple Event Asset")]
public class CinemachineEventAsset : ScriptableObject
{
    public bool lockPlayerInputWhileRunning = true;
    public bool restoreCameraAfter = true;
    public float postDelay = 0f;

    public List<EventStepDTO> steps = new List<EventStepDTO>();
}

public enum StepType
{
    LockInput,        // 입력 잠금/해제
    CameraFocus,      // 카메라 포커스 전환
    Dialogue,         // 대사 실행
    WaitEndFlag,      // 외부 EndEvent() 신호 대기
    Delay,            // 지연
    RestoreCamera     // 카메라 기본 복원
}

[System.Serializable]
public class EventStepDTO
{
    public StepType type;

    // 공통/옵션 필드(타입에 따라 일부만 사용)
    public bool lockOn;               // LockInput
    public int cameraSlot;            // CameraFocus
    public string fileName, groupName; // Dialogue
    public float seconds;             // Delay

    // 필요시 확장 필드(블렌드 힌트 등)
    // public BlendHint blendHint;
}
