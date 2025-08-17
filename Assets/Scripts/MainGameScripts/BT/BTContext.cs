using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BTContext
{
    public GameObject Owner { get; private set; }
    public Blackboard Blackboard { get; private set; }
    public float DeltaTime { get; internal set; }

    public BTContext(GameObject owner, Blackboard bb)
    {
        Owner = owner;
        Blackboard = bb;
    }
}