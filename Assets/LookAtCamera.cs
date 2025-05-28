using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform cam;
    void Start()
    {
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
    }
}
