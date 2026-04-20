using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float maxDistance;
    [SerializeField] private float movingSpeed;
    private bool followPlayer = true;
    private Camera cam;


    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (followPlayer)
        {
            CamFollowPlayer();
        }
        else
        {
            FollowMouse();
        }
    }

    public void SetFollowPlayer(bool newVal)
    {
        followPlayer = newVal;
    }

    private void CamFollowPlayer()
    {
        Vector3 newPos = new Vector3(player.position.x, player.position.y, this.transform.position.z);
        this.transform.position = newPos;
    }

    public void MoveCamera(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            followPlayer = false;
        }
        if (callbackContext.canceled)
        {
            followPlayer = true;
        }
    }

    private void FollowMouse()
    {
        Vector3 camPos = cam.ScreenToWorldPoint(Input.mousePosition);
        camPos.z = this.transform.position.z;
        Vector3 offset = camPos - new Vector3(player.position.x, player.position.y, camPos.z);
        if (offset.magnitude > maxDistance)
        {
            offset = offset.normalized * maxDistance;
        }
        Vector3 target = new Vector3(player.position.x, player.position.y, camPos.z) + offset;
        transform.position = Vector3.MoveTowards(transform.position, target, movingSpeed * Time.deltaTime);

    }
}