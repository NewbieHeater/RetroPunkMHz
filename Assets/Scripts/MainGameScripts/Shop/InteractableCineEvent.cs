using UnityEngine;

public class InteractableCineEvent : InteractableBase
{
    [Header("Cinemachine Event")]
    public CinemachineEventAsset eventAsset;
    private string npcId = "NPC";

    protected override bool OnInteract()
    {
        if (eventAsset == null)
        {
            Debug.LogWarning($"[InteractableCineEvent] {name}: eventAsset가 비어 있습니다.");
            return false;
        }

        // 이미 이벤트가 재생 중이면 중복 실행 방지
        if (CinemachineEventReader.Instance != null && CinemachineEventReader.Instance.IsRunning)
            return false;

        //CinemachineEventReader.Instance?.PlayEvent(eventAsset);
        GameProgress.I.SetFlag($"TalkedTo.{npcId}", true);
        return true;
    }
}
