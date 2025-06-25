using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private float checkHeight = 0.1f;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private float maxFallDistance = 0.3f;
    public LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public RaycastHit LastHit { get; private set; }

    private Animator animator;

    void Awake() { animator = GetComponentInChildren<Animator>(); }

    public void UpdateGroundStatus()
    {
        Vector3 origin = transform.position + Vector3.up * checkHeight;
        bool grounded = Physics.CheckSphere(origin, checkRadius, groundLayer, QueryTriggerInteraction.Ignore);
        RaycastHit hit = default;
        if (grounded)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit rHit, checkHeight + maxFallDistance, groundLayer, QueryTriggerInteraction.Ignore))
            {
                hit = rHit;
            }
        }
        IsGrounded = grounded;
        LastHit = hit;
        animator.SetBool("Grounded", grounded);
    }
}
