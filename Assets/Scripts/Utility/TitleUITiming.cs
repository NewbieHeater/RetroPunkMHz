using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleUITiming : MonoBehaviour
{
    [SerializeField]Animator UIanim;
    // Start is called before the first frame update
    public void StartUI()
    {
        UIanim.SetBool("Start", true);
    }
}
