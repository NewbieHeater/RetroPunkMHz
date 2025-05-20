using UnityEngine;

public class Attack : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Animator animator;
    int HashAttackCount = Animator.StringToHash("AttackCount");
    void Start()
    {
        TryGetComponent(out animator);
        //animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    public int AttackCount
    {
        get => animator.GetInteger(HashAttackCount);
        set => animator.SetInteger(HashAttackCount, value);
    }
}
