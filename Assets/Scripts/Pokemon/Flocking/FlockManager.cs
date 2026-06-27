using UnityEngine;
using System.Collections.Generic;

public class FlockManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private FlockAgent agentPrefab;
    [SerializeField] private int agentCount;
    [SerializeField] private Vector3 spawnExtents = new Vector3(10f, 5f, 10f);

    [Header("Movement")]
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxForce;

    [Header("Neighbours")]
    [SerializeField] private float neighboursRadious;
    [SerializeField] private float separationRadious;

    [Header("Weights")]
    [SerializeField] private float separationWeights;
    [SerializeField] private float aligmentWeights;
    [SerializeField] private float cohesionWeights;
    [SerializeField] private float targetWeights;
    [SerializeField] private float boundWeights;

    [Header("Targets")]
    [SerializeField] private List<Transform> wayPoints = new List<Transform>();
    [SerializeField] private float distanceToSwitchWayPoint;

    private Transform globalTarget;
    private int currentWayPointIndex = 0;
    private readonly List<FlockAgent> agents = new List<FlockAgent>();

    public List<FlockAgent> Agents => agents;
    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxSpeed;
    public float MaxForce => maxForce;
    public float NeighboursRadious => neighboursRadious;
    public float SeparationRadious => separationRadious;
    public float SeparationWeights => separationWeights;
    public float AligmentWeights => aligmentWeights;
    public float CohesionWeights => cohesionWeights;
    public float TargetWeights => targetWeights;
    public float BoundWeights => boundWeights;
    public Transform GlobalTarget => globalTarget;
    public Vector3 BoundsCenter => transform.position;
    public Vector3 BoundsExtents => spawnExtents;

    private void Start()
    {
        GameObject targetObj = new GameObject("Wishiwashi_FlockTarget");
        globalTarget = targetObj.transform;

        if (wayPoints.Count > 0)
        {
            globalTarget.position = wayPoints[currentWayPointIndex].position;
        }
        else
        {
            globalTarget.position = transform.position;
        }
        
        SpawnAgents();
    }

    private void Update()
    {
        if (wayPoints.Count == 0)
            return;

        Vector3 flockCenter = Vector3.zero;
        if (agents.Count > 0)
        {
            foreach (var agent in agents)
            {
                flockCenter += agent.transform.position;
            }
            flockCenter /= agents.Count;
        }

        float distanceToTarget = Vector3.Distance(flockCenter, wayPoints[currentWayPointIndex].position);
        if (distanceToTarget < distanceToSwitchWayPoint)
        {
            currentWayPointIndex = (currentWayPointIndex + 1) % wayPoints.Count;
            globalTarget.position = wayPoints[currentWayPointIndex].position;
        }
    }

    private void SpawnAgents()
    {
        for (int i = 0; i < agentCount; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnExtents.x, spawnExtents.x),
                Random.Range(-spawnExtents.y, spawnExtents.y),
                Random.Range(-spawnExtents.z, spawnExtents.z));

            Vector3 spawnPosition = transform.position + randomOffset;
            Quaternion spawnRotation = Random.rotation;

            FlockAgent newAgent = Instantiate(agentPrefab, spawnPosition, spawnRotation, transform);
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, spawnExtents * 2f);

        if (wayPoints.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < wayPoints.Count; i++)
            {
                if (wayPoints[i] != null)
                {
                    Gizmos.DrawSphere(wayPoints[i].position, 0.5f);
                    int next = (i + 1) % wayPoints.Count;
                    if (wayPoints[next] != null)
                        Gizmos.DrawLine(wayPoints[i].position, wayPoints[next].position);
                }
            }
        }
    }
}
