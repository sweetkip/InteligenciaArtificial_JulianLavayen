using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [Header("Settiings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraPivot;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    public Material HealthBar;
    private PlayerModel model;

    [Header("Stats")]
    [SerializeField] private int health = 3;
    [SerializeField] private int maxHealth = 3;

    [Header("PokéBall")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float upwardForce = 2f;


    private void Awake()
    {

        float vidaNormalizada = health / 3f;
        HealthBar.SetFloat("_Vida_anterior", vidaNormalizada);
        HealthBar.SetFloat("_Vida", vidaNormalizada);
        HealthBar.SetFloat("_tiempoHit", Time.time);
        model = GetComponent<PlayerModel>();
        Cursor.lockState = CursorLockMode.Locked;
        cameraYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        Look();
        Movement();


        if (Input.GetMouseButtonDown(0))
        {
            ThrowBall();
        }
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraYaw += mouseX;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -35f, 60f);
        cameraPivot.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
    }

    private void Movement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = cameraPivot.forward;
        Vector3 camRight = cameraPivot.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dir = (camForward * v + camRight * h).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.C);

        if (Input.GetKey(KeyCode.Space))
        {
            model.Jump();
        }

        model.Walk(dir, isRunning, isCrouching);
        if (dir.magnitude > 0.1f)
        {
            model.Rotate(dir);
        }
    }

    private void ThrowBall()
    {
        GameObject pokeBall = Instantiate(ballPrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = pokeBall.GetComponent<Rigidbody>();

        Vector3 forceToAdd = throwPoint.forward * throwForce + transform.up * upwardForce;

        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    public void TakeDamage()
    {
        float vidaAntesNormalizada = health / 3f;

        health--;
        Debug.Log("Remaining Life: " + health);
        float vidaNormalizada = health / 3f;
        HealthBar.SetFloat("_Vida_anterior", vidaAntesNormalizada);
        HealthBar.SetFloat("_Vida", vidaNormalizada);
        HealthBar.SetFloat("_tiempoHit", Time.time);

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
            float vidaAntesNormalizada = health / 3f;
            health++;
            Debug.Log("Remaining Life: " + health);
            float vidaNormalizada = health / 3f;
            HealthBar.SetFloat("_Vida_anterior", vidaAntesNormalizada);
            HealthBar.SetFloat("_Vida", vidaNormalizada);
            HealthBar.SetFloat("_tiempoHit", Time.time);
        }
    }
}