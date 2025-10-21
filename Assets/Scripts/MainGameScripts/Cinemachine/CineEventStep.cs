using System.Collections;
using UnityEngine;

public abstract class CineEventStep : ScriptableObject
{
    public abstract IEnumerator Execute(CineEventContext ctx);
}

// 카메라 포커스
[CreateAssetMenu(menuName = "CineEvent/Steps/CameraFocus")]
public class CameraFocusStep : CineEventStep
{
    public int cameraSlot;
    public bool restoreToDefaultAfter; // 선택

    public override IEnumerator Execute(CineEventContext ctx)
    {
        if (ctx.focusing != null)
        {
            if (ctx.savedCameraSlot < 0 && ctx.getCurrentCamSlot != null)
                ctx.savedCameraSlot = ctx.getCurrentCamSlot();

            ctx.focusing.FocusTo(cameraSlot);
        }
        yield break;
    }
}

// 대화 시작 & 완료까지 대기
[CreateAssetMenu(menuName = "CineEvent/Steps/Dialogue")]
public class DialogueStep : CineEventStep
{
    public string fileName;
    public string groupName;

    public override IEnumerator Execute(CineEventContext ctx)
    {
        yield return DialogueManager.Instance.StartDialogueAndWait(fileName, groupName);
    }
}

// 사용자 이벤트 종료 신호 대기 (EndEvent가 flag를 false로 만듦)
[CreateAssetMenu(menuName = "CineEvent/Steps/WaitEndFlag")]
public class WaitEndFlagStep : CineEventStep
{
    public override IEnumerator Execute(CineEventContext ctx)
    {
        ctx.endFlag = true;
        yield return new WaitUntil(() => ctx.endFlag == false);
    }
}

// 딜레이
[CreateAssetMenu(menuName = "CineEvent/Steps/Delay")]
public class DelayStep : CineEventStep
{
    public float seconds = 0f;
    public override IEnumerator Execute(CineEventContext ctx)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
    }
}

// 입력 잠금/해제
[CreateAssetMenu(menuName = "CineEvent/Steps/LockInput")]
public class LockInputStep : CineEventStep
{
    public bool lockOnEnter = true;   // true면 실행 시 잠금, false면 해제
    public override IEnumerator Execute(CineEventContext ctx)
    {
        ctx.setInputLocked?.Invoke(lockOnEnter);
        yield break;
    }
}

// 카메라 복원
[CreateAssetMenu(menuName = "CineEvent/Steps/RestoreCamera")]
public class RestoreCameraStep : CineEventStep
{
    public bool toDefault = true; // focusing.ToDefault 사용 여부
    public override IEnumerator Execute(CineEventContext ctx)
    {
        if (ctx.focusing != null)
        {
            if (toDefault) ctx.focusing.ToDefault();
            // 필요시 savedCameraSlot을 활용한 복원 로직도 구현 가능
        }
        yield break;
    }
}
