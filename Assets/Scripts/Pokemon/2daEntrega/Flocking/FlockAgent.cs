using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlockAgent : MonoBehaviour
{
    private FlockManager manager;
    private Rigidbody rb;

    public void Initialize(FlockManager flockManager)
    {
        manager = flockManager;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Start()
    {
        if (rb.linearVelocity == Vector3.zero)
        {
            Vector3 randomDir = Random.onUnitSphere;
            rb.linearVelocity = randomDir * Random.Range(manager.MinSpeed, manager.MaxSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (manager == null) return;

        Vector3 separation = CalculateSeparation();
        Vector3 aligment = CalculateAligment();
        Vector3 cohesion = CalculateCohesion();
        Vector3 targetForce = CalculateTargetForce();
        Vector3 boundsForce = CalculateBoundsForce();

        Vector3 steering =
            separation * manager.SeparationWeights +
            aligment * manager.AligmentWeights +
            cohesion * manager.CohesionWeights +
            targetForce * manager.TargetWeights +
            boundsForce * manager.BoundWeights;

        Vector3 acceleration = Vector3.ClampMagnitude(steering, manager.MaxForce);
        Vector3 newVelocity = rb.linearVelocity + acceleration * Time.fixedDeltaTime;

        float speed = newVelocity.magnitude;

        if (speed < manager.MinSpeed)
        {
            newVelocity = newVelocity.normalized * manager.MinSpeed;
        }
        else if (speed > manager.MaxSpeed)
        {
            newVelocity = newVelocity.normalized * manager.MaxSpeed;
;       }
        rb.linearVelocity = newVelocity;

        if (rb.linearVelocity.sqrMagnitude < 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));
        }
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 force = Vector3.zero;
        int count = 0;
        for (int i = 0; i < manager.Agents.Count; i++)
        {
            FlockAgent other = manager.Agents[i];

            if(other == this) continue;

            Vector3 offset = transform.position - other.transform.position;
            float distance = offset.magnitude;

            if (distance > 0f && distance < manager.SeparationRadious)
            {
                force += offset.normalized / distance;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        force /= count;
        return force.normalized;
    }


    private Vector3 CalculateAligment()
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;

        for (int i = 0; i < manager.Agents.Count; i ++)
        {
            FlockAgent other = manager.Agents[i];

            if (other == this) continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance < manager.NeighboursRadious)
            {
                averageVelocity += other.rb.linearVelocity;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        averageVelocity /= count;

        if(averageVelocity == Vector3.zero)
            return Vector3.zero;

        return averageVelocity.normalized;
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        for (int i = 0; i < manager.Agents.Count; i++)
        {
            FlockAgent other = manager.Agents[i];
            if (other == this) continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance < manager.NeighboursRadious)
            {
                center += other.rb.linearVelocity;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        center /= count;
        Vector3 dirToCenter = center - transform.position;

        if (dirToCenter == Vector3.zero)
            return Vector3.zero;

        return dirToCenter.normalized;
    }

    private Vector3 CalculateTargetForce()
    {
        if (manager.GlobalTarget == null)
            return Vector3.zero;

        Vector3 dir = manager.GlobalTarget.position - transform.position;

        if (dir == Vector3.zero)
            return Vector3.zero;

        return dir.normalized;
    }

    private Vector3 CalculateBoundsForce()
    {
        Vector3 center = manager.BoundsCenter;
        Vector3 extents = manager.BoundsExtents;
        Vector3 localOffset = transform.position - center;

        bool outsideX = Mathf.Abs(localOffset.x) > extents.x;
        bool outsideY = Mathf.Abs(localOffset.y) > extents.y;
        bool outsideZ = Mathf.Abs(localOffset.z) > extents.z;

        if (!outsideX && !outsideY && !outsideZ)
            return Vector3.zero;

        Vector3 dirToCenter = center - transform.position;

        if (dirToCenter == Vector3.zero)
            return Vector3.zero;

        return dirToCenter.normalized;
    }
}