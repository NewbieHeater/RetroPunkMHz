using UnityEngine;

[CreateAssetMenu(fileName = "CinemachineEvent", menuName = "Events/Cinemachine Event", order = 0)]
public class CinemachineEventAsset : ScriptableObject
{
    public enum EventMode
    {
        CameraAndDialogue,   // 카메라 전환 + 대사 출력
        DialogueOnly,        // 카메라 유지 + 대사 출력
        CameraOnly           // 카메라 전환 + 대사 없음
    }

    public enum BlendHint
    {
        Default,
        Cut,
        EaseInOut
        // 필요하면 확장
    }

    [Header("Basic")]
    public string eventId;
    public EventMode mode = EventMode.CameraAndDialogue;
    public bool lockPlayerInputWhileRunning = true;
    public bool oneShot = true;

    [Header("Camera")]
    [Tooltip("CinemachineFocusing.FocusTo(slot) 에 들어갈 슬롯/인덱스")]
    public int cameraSlot = 1;
    public BlendHint blendHint = BlendHint.Default;

    [Header("Dialogue")]
    [TextArea(3, 6)]
    public string dialogueScriptId;

    [Tooltip("이 이벤트가 끝난 후 원래 카메라로 복귀할지")]
    public bool restoreCameraAfter = false;
    public float postDelay = 0.25f; // 종료 후 살짝 대기
}
