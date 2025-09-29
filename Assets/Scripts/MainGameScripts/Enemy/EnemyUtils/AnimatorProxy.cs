using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AnimatorProxy : MonoBehaviour
{
    [SerializeField] Animator anim;
    public void PlayMove() => anim.Play("Move");
    public void RotateTowards(Quaternion target, float maxDeg) =>
        anim.transform.rotation = Quaternion.RotateTowards(anim.transform.rotation, target, maxDeg);
}