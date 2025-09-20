using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerForEnemys : MonoBehaviour
{
    private BoxCollider _attackCollider;
    private void Start()
    {
        _attackCollider = GetComponentInChildren<BoxCollider>();
    }

    public void OnAttackAnimationEnd()
    {
        _attackCollider.enabled = false;
    }
}
