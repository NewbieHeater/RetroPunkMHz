using UnityEngine;
using UnityEngine.UI;

public class SandBag : Enemy
{
    
    

    private bool isBroken = false;

    public override void OnExplosionInteract(Channel channel)
    {
        if (isBroken) return;

        // 요구사항을 만족하면 sandbag 파괴
        if (channel.amplitudePoints >= requiredAmpPts &&
            channel.periodPoints >= requiredPerPts &&
            channel.waveformPoints >= requiredWavPts)
        {
            BreakSandBag();
        }
        Debug.Log(channel.amplitudePoints);
        Debug.Log(channel.periodPoints);
        Debug.Log(channel.waveformPoints);
    }

    private void BreakSandBag()
    {
        isBroken = true;
        // 예: 파괴 애니메이션 재생
        //GetComponent<Animator>()?.SetTrigger("Break");
        // 콜라이더 비활성화
        GetComponent<Collider>().enabled = false;
        // 일정 시간 후 완전 제거
        Destroy(gameObject, 1.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (dead && collision.collider.CompareTag("Ground"))
        {
            Debug.Log("explod");
            Explode();
        }
    }
}
