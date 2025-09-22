using UnityEngine;

[DisallowMultipleComponent]
public class VisionSensor 
{

    Transform _player;
    public bool IsPlayerInSight(Transform owner, float range, LayerMask obstacleMask)
    {
        float dist = Vector3.Distance(owner.position, _player.transform.position);
        if (dist > range)
            return false;

        Vector3 toPlayer = (_player.transform.position - owner.position).normalized;
        float angle = Vector3.Angle(owner.transform.forward, toPlayer);
        Debug.DrawRay(owner.position + Vector3.up, toPlayer * dist, Color.green, 0.1f);
        if (angle > 50)
        {
            Debug.DrawRay(owner.position + Vector3.up, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        Vector3 origin = owner.position + Vector3.up;
        Vector3 target = _player.transform.position + Vector3.up;
        Vector3 dir = target - origin;
        Debug.DrawRay(origin, dir.normalized * range, Color.red, 0.1f);

        return IsRayHitOnPlayer(origin, dir, range);
    }

    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir, float distance, LayerMask obstacleMask)
    {
        Debug.DrawRay(origin, dir.normalized * distance, Color.red, 0.1f);

        if (Physics.Raycast(origin, dir, out var hit, distance, obstacleMask))
        {
            return hit;
        }

        return null;
    }
    LayerMask obstacleMask;
    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir, float maxDist)
    {
        if (GetRaycastHit(origin, dir, maxDist, obstacleMask) is RaycastHit hit)
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }
}
