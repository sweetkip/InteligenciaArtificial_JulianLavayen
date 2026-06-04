using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private LayerMask collisionMask;

    [SerializeField] private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (player == null)
            return;
        Vector3 desiredPosition = player.position
                                - player.forward * distance
                                + Vector3.up * height;

        Vector3 direction = (desiredPosition - player.position).normalized;
        float targetDistance = distance;

        RaycastHit hit;

        if (Physics.Linecast(player.position + Vector3.up * height,
                             desiredPosition,
                             out hit,
                             collisionMask))
        {
            targetDistance = hit.distance - 0.2f;
        }

        Vector3 finalPosition = player.position
                              - player.forward * targetDistance
                              + Vector3.up * height;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );

        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}