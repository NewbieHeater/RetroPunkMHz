using UnityEngine;

public class NpcTalkTrigger : MonoBehaviour
{
    [SerializeField] private string npcId = "ShopClerk";
    public void OnTalked()  // 대화 완료 시점에서 호출
    {
        GameProgress.I.SetFlag($"TalkedTo.{npcId}", true);
    }
}