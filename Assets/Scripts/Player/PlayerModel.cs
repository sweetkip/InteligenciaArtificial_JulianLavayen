using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    //References
    private Rigidbody rb;
    private Animator anim;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Crouch Settings")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private float normalHeigth = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private Vector3 normalCenter = new Vector3 (0f, 1f, 0f);
    [SerializeField] private Vector3 crouchCenter = new Vector3(0f, 0.5f, 0f);

    [SerializeField] private Transform visualPivot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    public void Walk(Vector3 dir, bool isRunning, bool isCrouching)
    {
        HandleCollider(isCrouching);

        float currentSpeed = walkSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isRunning)
        {
            currentSpeed = runSpeed;
        }

        Vector3 vel = dir * currentSpeed;
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;

        //Anim
        //0 = Idle, 1 = Walk, 2 = Run
        if (anim != null)
        {
            float animSpeed = dir.magnitude;
            if (isRunning && !isCrouching && dir.magnitude > 0.1f)
                animSpeed = 2f;

            anim.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
            anim.SetBool("isCrouching", isCrouching);
            anim.SetBool("isGrounded", IsGrounded());
        }
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (anim != null)
            {
                anim.SetTrigger("Jump");
            }
        }
    }

    public void Rotate(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        visualPivot.rotation = Quaternion.Lerp(visualPivot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.5f, groundLayer);
    }
    
    private void HandleCollider(bool isCrouching)
    {
        float targetHeight = isCrouching ? crouchHeight : normalHeigth;
        Vector3 targetCenter = isCrouching ? crouchCenter : normalCenter;

        if (playerCollider.height != targetHeight)
        {
            playerCollider.height = targetHeight;
            playerCollider.center = targetCenter;
        }
    }
}