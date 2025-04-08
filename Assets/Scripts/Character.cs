using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.VFX;

public class Character : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;
    

    private IInteractable currentInteractable;
    public LayerMask groundLayerMask;

    [Header("Player Stats")]
    [SerializeField] private int moveSpeed;
    [SerializeField] private int jumpPower;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float interactionRange = 3f; // ���� ����

    private float lastHor = 1;
    public Vector3 movement;

    public bool isJumping = false;
    public bool isGrounded = true;

    public Dictionary<string, IState<Character>> dicState = new Dictionary<string, IState<Character>>();
    public StateMachine<Character> sm;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        dicState.Add("Move", new MoveState());
        dicState.Add("Idle", new IdleState());
        dicState.Add("Jump", new JumpState());

        sm = new StateMachine<Character>(this, dicState["Idle"]);
    }

    public void Move()
    {
        float hor = Input.GetAxisRaw("Horizontal");

        // hor ���� 0�� �ƴ϶�� ������ �Է� ������ ����
        if (hor != 0)
            lastHor = hor;

        movement = new Vector3(hor, 0, 0).normalized;

        //animator.SetBool("Move", movement != Vector3.zero);

        rb.MovePosition(transform.position + movement * Time.deltaTime * moveSpeed);

        // hor�� 0�̸� lastHor�� ����� ȸ�� ���� ����
        float faceDir = (hor == 0 ? lastHor : hor);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * faceDir), rotationSpeed);
    }

    public void Jump()
    {
        rb.velocity = new Vector3(0, jumpPower, 0);

    }

    private void Update()
    {
        CheckGrounded();
        FindClosestInteractable();
        //if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        //{
        //    currentInteractable.Interact();
        //}
        sm.DoOperateUpdate();
    }
    private void FixedUpdate()
    {
        sm.DoOperateFixedUpdate();
    }

    void CheckGrounded()
    {
        // �÷��̾ ���� ��� ���̸� isGrounded�� false�� ó��
        if (rb.velocity.y > 0.1f)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.6f, groundLayerMask);
        }
    }

    // ���� ���� ���� ��� Collider�� �˻��Ͽ� IInteractable�� ������ ��ü �� ���� ����� ��ü�� ã��
    private void FindClosestInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);
        IInteractable closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = interactable;
                }
            }
        }

        currentInteractable = closest;
    }

    // �����Ϳ��� ��ȣ�ۿ� ������ �ð������� Ȯ���� �� �ֵ��� Gizmo�� ǥ��
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

}