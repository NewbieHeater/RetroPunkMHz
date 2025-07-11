using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowCommand : Command
{
    public GameObject BoxPrefab { get; private set; }
    public Vector3 SpawnPosition { get; private set; }
    public Quaternion SpawnRotation { get; private set; }
    public Vector3 LaunchForce { get; private set; }
    public ForceMode ForceMode { get; private set; }

    public ThrowCommand(
        GameObject boxPrefab,
        Vector3 spawnPosition,
        Quaternion spawnRotation,
        Vector3 launchForce,
        ForceMode forceMode = ForceMode.Impulse
    )
    {
        BoxPrefab = boxPrefab;
        SpawnPosition = spawnPosition;
        SpawnRotation = spawnRotation;
        LaunchForce = launchForce;
        ForceMode = forceMode;
    }

    public override void Execute()
    {
        GameObject box = GameObject.Instantiate(
            BoxPrefab,
            SpawnPosition,
            SpawnRotation
        );

        Rigidbody rb = box.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(LaunchForce, ForceMode);
    }
}
