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

    // maxhp, curhp���������� �ڸ��� �÷��̾� ü�°� ����ü�� �ֱ�
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
        //input.getkeydown ���  �ǰݽ� �ߵ��Ǵ� �������� �����ϱ�
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