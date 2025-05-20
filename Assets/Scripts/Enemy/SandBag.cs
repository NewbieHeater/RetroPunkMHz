using UnityEngine;
using UnityEngine.UI;

public class SandBag : EnemyBase
{
    private bool isBroken = false;

    public override void Patrol()
    {
        throw new System.NotImplementedException();
    }
    public override void Attack()
    {
        throw new System.NotImplementedException();
    }

    public override void OnExplosionInteract(Channel channel)
    {
        if (isBroken) return;

        // 요구사항을 만족하면 sandbag 파괴
        if (channel.amplitudePoints >= RequiredAmpPts &&
            channel.periodPoints >= RequiredPerPts &&
            channel.waveformPoints >= RequiredWavPts)
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

        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 1.5f);
    }




    
}
