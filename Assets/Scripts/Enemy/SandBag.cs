using UnityEngine;
using UnityEngine.UI;

public class SandBag : Enemy
{
    
    

    private bool isBroken = false;

    public override void OnExplosionInteract(Channel channel)
    {
        if (isBroken) return;

        // �䱸������ �����ϸ� sandbag �ı�
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




    private void OnCollisionEnter(Collision collision)
    {
        if (dead && collision.collider.CompareTag("Ground"))
        {
            Debug.Log("explod");
            Explode();
        }
    }
}
