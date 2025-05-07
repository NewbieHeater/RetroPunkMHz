using UnityEngine;

public class UnityService : IUnityService
{
    /// <summary>
    /// Global instance of default UnityService.
    /// </summary>
    /// <returns></returns>
    public static IUnityService Instance { get; } = new UnityService();

    /// <inheritdoc/>
    public float deltaTime => Time.deltaTime;

    /// <inheritdoc/>
    public float time => Time.time;

    /// <inheritdoc/>
    public float fixedDeltaTime => Time.fixedDeltaTime;
}