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
        Wander,
        Captured,
        Wimpod_Lake,
        Wimpod_Tower,
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
        Wimpod,
        Sandygast,
        Gimmighoul
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private LineOfSight los;
    [SerializeField] private AudioClip captureSound;
    [SerializeField] private AudioSource audioSource;
    private Rigidbody pkmnRb;

    [Header("Values")]
    [SerializeField] private State state;
    [SerializeField] private Personality personality;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxPredictionTime = 10f;

    [Header("Wander")]
    [SerializeField] private float wanderChangeInterval = 1.5f;
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
            case Personality.Wimpod:
                tree = PKMNDecisionTree.CreateWimpodTree();
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

        if (isFleeing)
        {
            state = State.Wimpod_Lake;
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
                dir = NaturalFlee();
                break;

            case State.Pursue:
                dir = SteeringBehaviours.Pursue(transform, player, playerRb, maxPredictionTime, slowRadious);
                break;

            case State.Seek:
                dir = SteeringBehaviours.Seek(transform, player.position);
                break;

            case State.Wimpod_Tower:
                TowerRotation();
                pkmnRb.linearVelocity = Vector3.zero;
                break;

            case State.Wimpod_Lake:
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

            case State.Sandygast_Moving:
                dir = SteeringBehaviours.FollowPath(transform, currentPathPoints, ref currentPathIndex, sandygastNodeDistanceThreshold);
                if (dir == Vector3.zero && currentPathIndex >=  currentPathPoints.Count)
                {
                    waitTimer = surfaceWaitTime;
                    SetState(State.Sandygast_Idle);
                }
                break;

            case State.Gimmighoul_SearchCoin:
                SearchNearestCoin();
                break;

            case State.Gimmighoul_MovingToCoin:
                dir = SteeringBehaviours.FollowPath(transform, thetaPathPoints, ref thetaPathIndex, gimmighoulNodeThreshold);
                if (dir == Vector3.zero && thetaPathIndex >= thetaPathPoints.Count)
                    SetState(State.Gimmighoul_SearchCoin);
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
        finalDir += SteeringBehaviours.Flee(transform, player.position) * 0.8f;
        finalDir += wanderDirection * 0.2f;
        return finalDir.normalized;
    }

    public void OnCaptured()
    {
        audioSource.PlayOneShot(captureSound);
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
        if (other.CompareTag("Water") && personality == Personality.Wimpod)
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
        state = State.Wimpod_Tower;
    }

    private void WimpodVisible(bool visible)
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renders) r.enabled = visible;
    }


    public void SandySubmerge()
    {
        if (circuitNodes.Count == 0 || state == State.Sandygast_Moving)
            return;

        Node startNode = GetClosestNode(transform.position);
        if (startNode == null)
            return;

        Node targetNode = startNode;
        if (circuitNodes.Count > 1)
        {
            int safetyNet = 0;
            while (targetNode == startNode && safetyNet < 10)
            {
                targetNode = circuitNodes[Random.Range(0, circuitNodes.Count)];
                safetyNet++;
            }
        }
        else
            targetNode = circuitNodes[0];

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

            if (sandygastBodyMesh != null)
                sandygastBodyMesh.localPosition = new Vector3(originalBodyLocalPos.x, undergroundYOffset, originalBodyLocalPos.z);

            SetState(State.Sandygast_Moving);
        }
    }
    
    private void EmergFromSand()
    {
        if (sandygastBodyMesh != null)
        {
            sandygastBodyMesh.localPosition = Vector3.Lerp(sandygastBodyMesh.localPosition, originalBodyLocalPos, Time.deltaTime * rotationSpeed);
        }
    }

    private void SearchNearestCoin()
    {
        GameObject coinObj = GameObject.FindWithTag("GimmighoulCoin");
        if (coinObj != null)
        {
            targetCoin = coinObj.transform;
            ThetaPath();
        }
        else
        {
            pkmnRb.linearVelocity = new Vector3(0, pkmnRb.linearVelocity.y, 0);
        }
    }

    private void ThetaPath()
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

    public void OnCoinCollected()
    {
        targetCoin = null;
        thetaPathPoints.Clear();

        SetState(State.Gimmighoul_SearchCoin);
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
}