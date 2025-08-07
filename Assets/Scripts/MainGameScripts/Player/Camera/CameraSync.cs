using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSync : MonoBehaviour
{
    private Camera _camera;
    public Camera SyncTarget;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        _camera.fieldOfView = SyncTarget.fieldOfView;
        _camera.transform.position = SyncTarget.transform.position;
        _camera.transform.rotation = SyncTarget.transform.rotation;
    }
}
