using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private Animator camAnim;
    [SerializeField] private Animator UIAnim;

    private void Start()
    {
        camAnim = GetComponent<Animator>();
    }

    //매개변수 2개로 하면 인스펙터에서 안보여서 그냥 함수 2개로 쪼갬
    public void SetCamAnimBoolTrue(string name)
    {
        camAnim.SetBool(name, true);
    }
    public void SetCamAnimBoolFalse(string name)
    {
        camAnim.SetBool(name, false);
    }

    public void SetUIAnimBoolTrue(string name)
    {
        UIAnim.SetBool(name, true);
    }
    public void SetUIAnimBoolFalse(string name)
    {
        UIAnim.SetBool(name, false);
    }
}
