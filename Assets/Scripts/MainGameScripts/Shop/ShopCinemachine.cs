using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCinemachine : MonoBehaviour
{
    public GameObject shopUI;                     // 상점 UI
    public CinemachineVirtualCamera shopCamera;   // 상점용 카메라
    public CinemachineVirtualCamera mainCamera;   // 기본 카메라


    void Update()
    {
        if (shopUI == null || shopCamera == null || mainCamera == null)
            return;

        if (shopUI.activeSelf)
        {
            
            // UI가 켜져 있으면 상점 카메라 활성화
            shopCamera.Priority = 20;
            mainCamera.Priority = 5;

            shopCamera.m_Lens.OrthographicSize = 2f;
        }
        else
        {
            
            // UI가 꺼져 있으면 기본 카메라 활성화
            mainCamera.Priority = 20;
            shopCamera.Priority = 5;
        }
    }
}
