using System.Collections.Generic;
using UnityEngine;

public class PKMNController : MonoBehaviour
{
    public enum State
    {
        Arrive,
        Attack,
        Evade,
        Flee,
        Pursue,
        Seek,
        ToLake,
        Tower,
        Wander,
        Captured,
        Sandygast_Idle,
        Sandygast_Moving,
        Gimmighoul_SearchCoin,
        Gimmighoul_MovingToCoin
    }

    public enum Personality
    {
        Aggresive,
        Attack,
        Coward,
        Panic,
        Neutral,
        Tower,
        Sandygast,
        Gimmighoul
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private LineOfSight los;
    private Rigidbody pkmnRb;

    [Header("Values")]
    [SerializeField] private State state;
    [SerializeField] private Personality personality;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxPredictionTime = 10f;
    [SerializeField] private float maxAngleChange = 90f;

    [Header("Wander")]
    [SerializeField] private float wanderChangeInterval = 1.5f;
    [SerializeField] private float wanderTurnSpeed = 30f;
    private Vector3 wanderDirection;
    private float wanderTimer;

    [Header("Arrive")]
    [SerializeField] private float slowRadious = 5f;

    [Header("Tinkaton")]
    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("Wimpod")]
    [SerializeField] private Transform lakeTarget;
    [SerializeField] private float rotationInterval = 3f;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private Transform initialPos;
    private float nextRotationTime;
    private float targetYaw;
    private bool isFleeing = false;

    [Header("Sandygast")]
    [SerializeField] private Transform sandygastBodyMesh;
    [SerializeField] private float undergroundYOffset = -0.5f;
    [SerializeField] private float sandygastNodeDistanceThreshold = 0.5f;
    [SerializeField] private float surfaceWaitTime = 2f;
    private float waitTimer = 0f;
    private List<Node> circuitNodes = new List<Node>();
    private List<Vector3> currentPathPoints = new List<Vector3>();
    private int currentPathIndex = 0;
    private Vector3 originalBodyLocalPos;
    private bool isUnderground = false;
    public bool CanSearchPlayer => waitTimer <= 0f;

    [Header("Gimmighoul")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float gimmighoulNodeThreshold = 0.4f;
    private Transform targetCoin;
    private List<Vector3> thetaPathPoints = new List<Vector3>();
    private int thetaPathIndex = 0;
    public bool HasTargetCoin => targetCoin != null;

    [Header("Tree")]
    private Node_Decision tree;
    private PKMNContext context;

    private void Awake()
    {
        pkmnRb = GetComponent<Rigidbody>();
        los = GetComponent<LineOfSight>();
        if (player != null)
            playerRb = player.GetComponent<Rigidbody>();

        wanderDirection = transform.forward;
        wanderTimer = 0f;

        if(sandygastBodyMesh != null)
            originalBodyLocalPos = sandygastBodyMesh.localPosition;

        Node[] foundNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        circuitNodes.AddRange(foundNodes);
    }

    private void Start()
    {
        context = new PKMNContext { self = transform, player = player, los = los };

        switch (personality)
        {
            case Personality.Aggresive:
                tree = PKMNDecisionTree.CreateAggresiveTree();
                break;
            case Personality.Attack:
                tree = PKMNDecisionTree.CreateAttackTree(attackRange);
                break;
            case Personality.Coward:
                tree = PKMNDecisionTree.CreateCowardTree();
                break;
            case Personality.Panic:
                tree = PKMNDecisionTree.CreatePanicTree();
                break;
            case Personality.Tower:
                tree = PKMNDecisionTree.CreateTowerTree();
                break;
            case Personality.Sandygast:
                tree = PKMNDecisionTree.CreateSandygastTree();
                break;
            case Personality.Gimmighoul:
                tree = PKMNDecisionTree.CreateGimmighoulTree();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (state == State.Captured)
            return;

        if (state == State.Gimmighoul_MovingToCoin)
        {
            GimmighoulMoveToCoin();
            return;
        }
        if (state == State.Sandygast_Moving)
        {
            SandygastMoveUnderground();
            return;
        }
        if (isFleeing)
        {
            state = State.ToLake;
        }
        else
        {
            if (state == State.Sandygast_Idle && waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
            }

            tree.Evaluate(this, context);
        }

        Vector3 dir = Vector3.zero;

        switch (state)
        {
            case State.Arrive:
                dir = SteeringBehaviours.Arrive(transform, player.position, 5f);
                break;

            case State.Attack:
                Attack();
                dir = Vector3.zero;
                break;

            case State.Evade:
                dir = SteeringBehaviours.Evade(transform, player, playerRb, maxPredictionTime, slowRadious);
                break;

            case State.Flee:
                dir = SteeringBehaviours.Flee(transform, player.position);
                NaturalFlee();
                break;

            case State.Pursue:
                dir = SteeringBehaviours.Pursue(transform, player, playerRb, maxPredictionTime, slowRadious);
                break;

            case State.Seek:
                dir = SteeringBehaviours.Seek(transform, player.position);
                break;

            case State.Tower:
                TowerRotation();
                pkmnRb.linearVelocity = Vector3.zero;
                break;

            case State.ToLake:
                isFleeing = true;
                dir = SteeringBehaviours.Seek(transform, lakeTarget.position);
                break;

            case State.Wander:
                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0f)
                {
                    wanderDirection = SteeringBehaviours.Wander(wanderDirection, 180f);
                    wanderTimer = wanderChangeInterval;
                }
                dir = wanderDirection;
                break;

            case State.Sandygast_Idle:
                pkmnRb.linearVelocity = new Vector3(0, pkmnRb.linearVelocity.y, 0);
                EmergFromSand();
                break;

            case State.Gimmighoul_SearchCoin:
                SearchNearestCoin();
                break;
        }
        Move(dir);
    }

    private void Move(Vector3 dir)
    {
        
        Vector3 hSpeed = dir * speed;
        float currentVSpeed = pkmnRb.linearVelocity.y;

        pkmnRb.linearVelocity = new Vector3(hSpeed.x, currentVSpeed, hSpeed.z);

        if (dir.sqrMagnitude > 0.05f)
        {
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

    }

    public void SetState(State newState)
    {
        if (state == newState)
        {
            return;
        }

        state = newState;
    }

    private Vector3 NaturalFlee()
    {
        Vector3 finalDir = Vector3.zero;
        
        if (state == State.Flee)
        {
            finalDir += SteeringBehaviours.Flee(transform, player.position) * 0.8f;

            finalDir += wanderDirection * 0.2f;
        }
        return finalDir.normalized;
    }

    public void OnCaptured()
    {
        SetState(State.Captured);

        pkmnRb.linearVelocity = Vector3.zero;
        pkmnRb.isKinematic = true;

        gameObject.SetActive(false);
    }

    private void Attack()
    {
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * rotationSpeed);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Instantiate(hammerPrefab, attackPoint.position, transform.rotation);
            lastAttackTime = Time.time;
        }
    }

    private void TowerRotation()
    {
        if (Time.time >= nextRotationTime)
        {
            targetYaw += 90f;
            nextRotationTime = Time.time + rotationInterval;
        }

        Quaternion targetRot = Quaternion.Euler(0, targetYaw, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water") && personality == Personality.Tower)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        WimpodVisible(false);
        state = State.Captured;

        yield return new WaitForSeconds(respawnTime);

        transform.position = initialPos.position;
        targetYaw = 0;
        isFleeing = false;

        WimpodVisible(true);
        state = State.Tower;
    }

    private void WimpodVisible(bool visible)
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renders) r.enabled = visible;
    }

    public void StartSandygastSubmergeAndPathfind()
    {
        if (circuitNodes.Count == 0 || state == State.Sandygast_Moving) return;

        Node startNode = GetClosestNode(transform.position);
        if (startNode == null) return;

        Node targetNode = startNode;
        int safetyNet = 0;
        while (targetNode == startNode && safetyNet < 10)
        {
            targetNode = circuitNodes[Random.Range(0, circuitNodes.Count)];
            safetyNet++;
        }

        List<Node> path = AStar.Run(
            startNode,
            node => node == targetNode,
            node => node.neighbours,
            (n1, n2) => Vector3.Distance(n1.transform.position, n2.transform.position),
            node => Vector3.Distance(node.transform.position, targetNode.transform.position)
        );

        if (path != null && path.Count > 0)
        {
            currentPathPoints.Clear();
            foreach (var node in path)
            {
                currentPathPoints.Add(node.transform.position);
            }
            currentPathIndex = 0;
            state = State.Sandygast_Moving;
            isUnderground = true;
        }
    }

    public void SandygastMoveUnderground()
    {
        if (sandygastBodyMesh != null)
        {
            sandygastBodyMesh.localPosition = Vector3.Lerp(sandygastBodyMesh.localPosition,
                new Vector3(originalBodyLocalPos.x, undergroundYOffset, originalBodyLocalPos.z), Time.deltaTime * 5f);
        }

        if (currentPathPoints == null || currentPathIndex >= currentPathPoints.Count)
        {
            EndSandygastMovement();
            return;
        }

        Vector3 targetPoint = currentPathPoints[currentPathIndex];
        targetPoint.y = transform.position.y;

        if (Vector3.Distance(transform.position, targetPoint) < sandygastNodeDistanceThreshold)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPathPoints.Count)
            {
                EndSandygastMovement();
                return;
            }
        }

        Vector3 dir = SteeringBehaviours.Seek(transform, currentPathPoints[currentPathIndex]);
        Move(dir);
    }

    private void EmergFromSand()
    {
        if (sandygastBodyMesh != null)
        {
            sandygastBodyMesh.localPosition = Vector3.Lerp(sandygastBodyMesh.localPosition, originalBodyLocalPos, Time.deltaTime * 5f);
        }
    }

    private Node GetClosestNode(Vector3 position)
    {
        Node closest = null;
        float nearDistance = Mathf.Infinity;
        foreach (Node node in circuitNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < nearDistance)
            {
                nearDistance = distance;
                closest = node;
            }
        }
        return closest;
    }

    private void EndSandygastMovement()
    {
        pkmnRb.linearVelocity = new Vector3(0, pkmnRb.linearVelocity.y, 0);
        state = State.Sandygast_Idle;
        isUnderground = false;

        waitTimer = surfaceWaitTime;
    }

    private void SearchNearestCoin()
    {
        GameObject coinObj = GameObject.FindWithTag("GimmighoulCoin");
        if (coinObj != null)
        {
            targetCoin = coinObj.transform;
            CalculateThetaStarPath();
        }
        else
        {
            pkmnRb.linearVelocity = new Vector3(0, pkmnRb.linearVelocity.y, 0);
        }
    }

    private void CalculateThetaStarPath()
    {
        if (targetCoin == null) return;

        Node startNode = GetClosestNode(transform.position);
        Node targetNode = GetClosestNode(targetCoin.position);

        if (startNode == null || targetNode == null) return;

        List<Node> path = ThetaStar.Run(
            startNode,
            node => node == targetNode,
            node => node.neighbours,
            (n1, n2) => Vector3.Distance(n1.transform.position, n2.transform.position),
            node => Vector3.Distance(node.transform.position, targetNode.transform.position),
            (n1, n2) => {
                Vector3 dir = n2.transform.position - n1.transform.position;
                return !Physics.Raycast(n1.transform.position + Vector3.up * 0.2f, dir.normalized, dir.magnitude, wallLayerMask);
            }
        );

        if (path != null && path.Count > 0)
        {
            thetaPathPoints.Clear();
            foreach (var node in path)
            {
                thetaPathPoints.Add(node.transform.position);
            }
            thetaPathPoints.Add(targetCoin.position);
            thetaPathIndex = 0;
            state = State.Gimmighoul_MovingToCoin;
        }
    }

    private void GimmighoulMoveToCoin()
    {
        if (targetCoin == null || thetaPathIndex >= thetaPathPoints.Count)
        {
            state = State.Gimmighoul_SearchCoin;
            return;
        }

        Vector3 currentTarget = thetaPathPoints[thetaPathIndex];
        currentTarget.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, currentTarget);

        if (distance < gimmighoulNodeThreshold)
        {
            thetaPathIndex++;
            if (thetaPathIndex >= thetaPathPoints.Count)
            {
                pkmnRb.linearVelocity = new Vector3(0, pkmnRb.linearVelocity.y, 0);
                state = State.Gimmighoul_SearchCoin;
                return;
            }
            return;
        }

        Vector3 dir = SteeringBehaviours.Seek(transform, thetaPathPoints[thetaPathIndex]);

        if (dir.sqrMagnitude > 0.01f)
        {
            Move(dir);
        }
        else
        {
            pkmnRb.linearVelocity = new Vector3(0, pkmnRb.linearVelocity.y, 0);
        }
    }

    public void OnCoinCollected()
    {
        targetCoin = null;
        thetaPathPoints.Clear();

        thetaPathIndex = 0;
        if (pkmnRb != null)
        {
            pkmnRb.linearVelocity = Vector3.zero;
            pkmnRb.angularVelocity = Vector3.zero;
        }

        state = State.Gimmighoul_SearchCoin;
    }
}