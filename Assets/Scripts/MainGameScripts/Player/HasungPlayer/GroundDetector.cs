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
    Vector3 boxSize;
    void Awake() 
    { 
        animator = GetComponentInChildren<Animator>();
        boxSize = new Vector3(0.4f, 0.1f, 0.4f);
    }
    [SerializeField] Transform groundCheck;

    //public bool IsGrounded()
    //{
    //    Vector3 boxSize = new Vector3(transform.lossyScale.x, 0.4f, transform.lossyScale.z);
    //    return Physics.CheckBox(groundCheck.position, boxSize, Quaternion.identity, groundLayer);
    //    //Physics.CheckSphere(origin, checkRadius, groundLayer, QueryTriggerInteraction.Ignore);
    //}

    // Quaternion.identity는 회전값이 없다는 의미입니다.
    public void UpdateGroundStatus()
    {
        Vector3 origin = transform.position + Vector3.up * checkHeight;
        bool grounded = Physics.Raycast(origin, Vector3.down, out RaycastHit rHit, checkHeight + maxFallDistance, groundLayer, QueryTriggerInteraction.Ignore);
        RaycastHit hit = default;
        //if (grounded)
        //{
        //    if (Physics.Raycast(origin, Vector3.down, out RaycastHit rHit, checkHeight + maxFallDistance, groundLayer, QueryTriggerInteraction.Ignore))
        //    {
        //        hit = rHit;
        //    }
        //}
        hit = rHit;
        IsGrounded = grounded;
        LastHit = hit;
        animator.SetBool("Grounded", grounded);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, boxSize);
    }
}
