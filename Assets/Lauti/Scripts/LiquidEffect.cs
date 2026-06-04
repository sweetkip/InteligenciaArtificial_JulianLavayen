using UnityEngine;

public class LiquidEffect : MonoBehaviour
{
    Renderer rend;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;
    Vector3 angularVelocity;
    [SerializeField] private float maxRotation = 0.03f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float Recovery = 1f;
    float rotationAmountX;
    float rotationAmountZ;
    float rotationAmountToAddX;
    float rotationAmountToAddZ;
    float pulse;
    float time = 0.5f;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }
    private void Update()
    {
        time += Time.deltaTime;

        rotationAmountToAddX = Mathf.Lerp(rotationAmountToAddX, 0, Time.deltaTime * (Recovery));
        rotationAmountToAddZ = Mathf.Lerp(rotationAmountToAddZ, 0, Time.deltaTime * (Recovery));


        pulse = 2 * Mathf.PI * rotationSpeed;
        rotationAmountX = rotationAmountToAddX * Mathf.Sin(pulse * time);
        rotationAmountZ = rotationAmountToAddZ * Mathf.Sin(pulse * time);


        rend.material.SetFloat("_RotationX", rotationAmountX);
        rend.material.SetFloat("_RotationZ", rotationAmountZ);


        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;


        rotationAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * maxRotation, -maxRotation, maxRotation);
        rotationAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * maxRotation, -maxRotation, maxRotation);


        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }

}
