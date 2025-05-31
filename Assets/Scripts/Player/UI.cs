using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField]
    private Slider hpbar;

    public float maxhp;
    public float curhp;
    float imsi;

    // maxhp, curhp변수선언한 자리에 플레이어 체력과 현재체력 넣기
    void Start()
    {
        maxhp = 100;
        curhp = maxhp;
        imsi = curhp / maxhp;
        hpbar.value = imsi;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        //input.getkeydown 대신  피격시 발동되는 조건으로 수정하기
        {
            if (curhp > 0)
            {
                curhp -= 10;
            }
            else
            {
                curhp = 0;
            }
            imsi = (float)curhp / (float)maxhp;
        }
        HandleHp();
    }
    private void HandleHp()
    {
        hpbar.value = Mathf.Lerp(hpbar.value, imsi, Time.deltaTime * 10);
    }
}