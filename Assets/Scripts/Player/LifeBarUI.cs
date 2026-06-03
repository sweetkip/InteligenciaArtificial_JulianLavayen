using UnityEngine;

public class BillboardY : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 targetPos = Camera.main.transform.position;
        targetPos.y = transform.position.y;

        transform.LookAt(targetPos);

        transform.Rotate(90f, 0f, 0f);
    }
}