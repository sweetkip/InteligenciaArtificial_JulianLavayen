using UnityEngine;

public class HealingPill : MonoBehaviour
{
    [SerializeField] private float offset = 2;
    [SerializeField] private float speed = 1;
    private NewControls playerController;
    private Vector3 initialPosition;
    private bool goRight;
    void Start()
    {
        initialPosition = transform.position;
        goRight = true;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        if (goRight)
        {
            pos.x += speed * Time.deltaTime;
            if (pos.x >= initialPosition.x + offset)
            {
                goRight = false;
            }
        }
        else
        {
            pos.x -= speed * Time.deltaTime;
            if (pos.x <= initialPosition.x - offset)
            {
                goRight = true;
            }
        }

        transform.position = pos;
    }
    private void OnTriggerEnter(Collider other)
    {
        playerController = other.GetComponent<NewControls>();
        if (playerController != null )
        {
            playerController.HealPlayer();
            Destroy(this.gameObject);
        }
    }
}
