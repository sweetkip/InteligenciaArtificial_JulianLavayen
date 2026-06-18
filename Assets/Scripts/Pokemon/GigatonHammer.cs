using UnityEngine;

public class GigatonHammer : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 700f;
    [SerializeField] private float duration = 1f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NewControls player = other.GetComponent<NewControls>();
            if (player != null)
            {
                player.TakeDamage();
                Destroy(gameObject);
            }    
        }
    }
}