using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneCameraControl : MonoBehaviour
{
    private CinemachineFocusing _focusing;
    public int a;
    void Start()
    {
        _focusing = GetComponent<CinemachineFocusing>();
    }

    
    void Update()
    {
        _focusing.FocusTo(a);

    }

}
