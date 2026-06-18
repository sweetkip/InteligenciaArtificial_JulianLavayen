using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NewControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Look")]
    [SerializeField] private Transform cameraTrans;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float upperLookLimit = -80f;
    [SerializeField] private float lowerLookLimit = 80f;

    [Header("Crouch")]
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private Vector3 normalCenter = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 crouchCenter = new Vector3(0f, 0.5f, 0f);

    [Header("Vignette")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float vignetteDefault = 0.5f;
    [SerializeField] private float vignetteCrouch = 0.5f;
    [SerializeField] private float vignetteSpeed = 5f;

    [Header("Stats")]
    [SerializeField] private int health = 3;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Material healthBar;

    [Header("PokéBall")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float upwardForce = 2f;

    [Header("References")]
    [SerializeField] private Animator anim;

    private CharacterController characterController;
    private AudioSource audioSource;
    private PlayerControls playerControls;

    private Vignette vignette;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    private bool isRunning = false;
    private bool isCrouching = false;
    private bool insideObstacle = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        playerControls = new PlayerControls();

        if (globalVolume != null && globalVolume.profile.TryGet<Vignette>(out var tempVignette))
        {
            vignette = tempVignette;
            vignetteDefault = vignette.intensity.value;
        }

        if (healthBar != null)
        {
            float normalizedHealth = health / 3f;
            healthBar.SetFloat("_PreviousHealth", normalizedHealth);
            healthBar.SetFloat("_Health", normalizedHealth);
            healthBar.SetFloat("_hitTime", Time.time);
        }
    }

    private void OnEnable()
    {
        playerControls.Enable();

        playerControls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerControls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        playerControls.Player.Jump.performed += ctx => Jump();

        playerControls.Player.Run.performed += ctx => isRunning = true;
        playerControls.Player.Run.canceled += ctx => isRunning = false;

        playerControls.Player.Crouch.performed += ctx => SetCrouchState(true);
        playerControls.Player.Crouch.canceled += ctx => SetCrouchState(false);

        playerControls.Player.Throw.performed += ctx => ThrowBall();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        LookAround();
        Movement();
        VignetteStealth();
    }

    private void Movement()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = walkSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isRunning)
        {
            currentSpeed = runSpeed;
        }

        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (anim != null)
        {
            float animSpeed = moveInput.magnitude;
            if (isRunning && !isCrouching && moveInput.magnitude > 0.1f)
            {
                animSpeed = 2f;
            }

            anim.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
            anim.SetBool("isCrouching", isCrouching);
            anim.SetBool("isGrounded", characterController.isGrounded);
        }
    }

    private void LookAround()
    {
        if (cameraTrans == null) return;

        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, upperLookLimit, lowerLookLimit);

        cameraTrans.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void Jump()
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (anim != null)
            {
                anim.SetTrigger("Jump");
            }
        }
    }

    private void SetCrouchState(bool crouchState)
    {
        isCrouching = crouchState;

        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        Vector3 targetCenter = isCrouching ? crouchCenter : normalCenter;

        if (characterController.height != targetHeight)
        {
            characterController.height = targetHeight;
            characterController.center = targetCenter;
        }
    }

    private void VignetteStealth()
    {
        if (vignette == null) return;

        float targetIntensity = (isCrouching && insideObstacle) ? vignetteCrouch : vignetteDefault;
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * vignetteSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            insideObstacle = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            insideObstacle = false;
        }
    }

    private void ThrowBall()
    {
        if (ballPrefab == null || throwPoint == null) return;

        GameObject pokeBall = Instantiate(ballPrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody ballRb = pokeBall.GetComponent<Rigidbody>();

        if (ballRb != null)
        {
            Vector3 forceToAdd = throwPoint.forward * throwForce + transform.up * upwardForce;
            ballRb.AddForce(forceToAdd, ForceMode.Impulse);
        }
    }

    public void TakeDamage()
    {
        float previousNormalizedHealth = health / 3f;

        health--;
        Debug.Log("Remaining Life: " + health);
        float normalizedHealth = health / 3f;

        if (healthBar != null)
        {
            healthBar.SetFloat("_PreviousHealth", previousNormalizedHealth);
            healthBar.SetFloat("_Health", normalizedHealth);
            healthBar.SetFloat("_hitTime", Time.time);
        }

        if (health <= 0 && SceneController.instance != null)
        {
            SceneController.instance.TriggerDefeat();
        }
    }

    public void HealPlayer()
    {
        if (health < maxHealth)
        {
            Debug.Log("Player curado");
            float previousNormalizedHealth = health / 3f;
            health++;
            Debug.Log("Remaining Life: " + health);
            float normalizedHealth = health / 3f;

            if (healthBar != null)
            {
                healthBar.SetFloat("_PreviousHealth", previousNormalizedHealth);
                healthBar.SetFloat("_Health", normalizedHealth);
                healthBar.SetFloat("_hitTime", Time.time);
            }
        }
    }
}