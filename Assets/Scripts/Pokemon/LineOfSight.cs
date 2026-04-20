using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private int distance;
    [SerializeField] private int angle;
    [SerializeField] private LayerMask layerMask;

    public bool isInRange(Transform self, Transform target)
    {
        return Vector3.Distance(self.position, target.position) <= distance;
    }

    public bool isInAngle(Transform self, Transform target)
    {
        Vector3 dir = (target.position - self.position).normalized;
        return Vector3.Angle(self.forward, dir) <= angle / 2;
    }

    public bool hasLineOfSight(Transform self, Transform target)
    {
        Vector3 dir = target.position - self.position;
        return !Physics.Raycast(self.position, dir, dir.magnitude, layerMask);
    }

    public bool LOS(Transform self, Transform target)
    {
        bool los = isInRange(self, target) & isInRange(self, target) & hasLineOfSight(self, target);
        return los;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.pink;
        Gizmos.DrawWireSphere(transform.position, distance);
        Vector3 leftBoundary = Quaternion.AngleAxis(-angle / 2f, Vector3.up) * transform.forward;
        Vector3 rightBoundary = Quaternion.AngleAxis(angle / 2f, Vector3.up) * transform.forward;

        Gizmos.color = Color.lightBlue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * distance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * distance);
    }
}