using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineFocusing : MonoBehaviour
{
    public CinemachineVirtualCamera[] vcams;

    public void FocusTo(int cam)
    {
        for(int i = 0; i < vcams.Length; i++)
        {
            if (vcams[i] == null) continue;
            if(i == cam)
            {
                vcams[i].Priority = 10;
                Debug.Log("cam" + i + " focused");
                continue;
            }
            vcams[i].Priority = 5;
            Debug.Log("cam" + i + " deactivated");
        }
    }
    public void ToDefault()
    {
        vcams[0].Priority = 10;
        for (int i = 1; i < vcams.Length; i++)
        {
            if (vcams[i] == null) continue;
            vcams[i].Priority = 5;
        }
    }
}
