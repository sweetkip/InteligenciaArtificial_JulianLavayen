using UnityEngine;
using System.Collections.Generic;

public class FlockManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private FlockAgent agentPrefab;
    [SerializeField] private int agentCount = 20;
    [SerializeField] private Vector3 spawnExtents = new Vector3(10f, 5f, 10f);

    [Header("Movement")]
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxForce = 10f;

    [Header("Neighbours")]
    [SerializeField] private float neighboursRadious = 4f;
    [SerializeField] private float separationRadious = 2f;

    [Header("Weights")]
    [SerializeField] private float separationWeights = 2f;
    [SerializeField] private float aligmentWeights = 1f;
    [SerializeField] private float cohesionWeights = 1f;
    [SerializeField] private float targetWeights = 0.6f;
    [SerializeField] private float boundWeights = 1.5f;

    [Header("Optional Target")]
    [SerializeField] private Transform globalTarget;

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
        SpawnAgents();
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
    }
}
