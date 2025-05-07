using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnityService
{
    /// <summary>
    /// Get the current delta time in seconds between this and last frame
    /// </summary>
    /// <returns>The current delta time between this and the previous frame</returns>
    float deltaTime { get; }

    /// <summary>
    /// Get the current fixed delta time for physics based update
    /// </summary>
    /// <returns>the delta time shown by the fixed delta time</returns>
    float fixedDeltaTime { get; }

    /// <summary>
    /// Gets the current time in seconds since start of the game
    /// </summary>
    /// <value>Time in seconds since the start of the game</value>
    float time { get; }
}