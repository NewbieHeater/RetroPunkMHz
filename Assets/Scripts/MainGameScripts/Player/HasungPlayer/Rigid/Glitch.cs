using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Glitch : MonoBehaviour
{
    [Header("Glitch Pass Settings")]
    [SerializeField] private float teleportSpace = 0.5f;     // X thickness threshold
    [SerializeField] private float postTeleportOffset = 1.2f;
    [SerializeField] private float teleportCooldown = 0.08f;
    [Tooltip("Colliders with this Tag trigger glitch-pass. Alternatively, replace with LayerMask if preferred.")]
    [SerializeField] private string glitchTag = "Glitch";

    public bool CanPass { get; set; } = false;

    private Rigidbody _rb;
    private float _lastTeleportTime = -999f;
    private int _lastGlitchId = -1;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanPass) return;
        Collider wallCol = collision.collider;
        if (!wallCol || !wallCol.CompareTag(glitchTag)) return;

        int id = wallCol.GetInstanceID();
        if (id == _lastGlitchId && Time.time - _lastTeleportTime < teleportCooldown)
            return;

        Bounds wallBounds = wallCol.bounds;
        float width = wallBounds.size.x;
        if (width >= teleportSpace) return;

        float px = _rb.position.x;
        float minX = wallBounds.min.x;
        float maxX = wallBounds.max.x;
        float targetX = (Mathf.Abs(px - minX) < Mathf.Abs(px - maxX)) ? maxX : minX;

        float offset = (px < targetX) ? postTeleportOffset : -postTeleportOffset;
        Vector3 dest = new Vector3(targetX + offset, _rb.position.y, _rb.position.z);

        // Safer physics move in FixedUpdate context
        _rb.MovePosition(dest);

        _lastGlitchId = id;
        _lastTeleportTime = Time.time;
    }
}
