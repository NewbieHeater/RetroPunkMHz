using UnityEngine;

public class RaycastHelper : IRaycastHelper
{
    /// <summary>
    /// Universal instance of RaycastHelper.
    /// </summary>
    public static RaycastHelper Instance = new RaycastHelper();

    /// <summary>
    /// Private instance of raycast helper.
    /// </summary>
    private RaycastHelper() { }

    /// <inheritdoc/>
    public bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction)
    {
        bool didHit = Physics.Raycast(new Ray(source, direction), out RaycastHit hit, distance, layerMask, queryTriggerInteraction);
        stepHit = new RaycastHitWrapper(hit);
        return didHit;
    }
}