using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
    [Header("�κ��丮 ����")]
    [SerializeField] private InventoryMain mInventoryMain;

    [Header("ȹ���� ������")]
    [SerializeField] private Item mHPItem, mManaItem;


    private void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 300, 40), "ü������ ������ ȹ��"))
        {
            mInventoryMain.AcquireItem(mHPItem);
        }

        if (GUI.Button(new Rect(400, 20, 300, 40), "�������� ������ ȹ��"))
        {
            mInventoryMain.AcquireItem(mManaItem);
        }
    }
}