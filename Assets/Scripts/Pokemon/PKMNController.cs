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
        Captured
    }

    public enum Personality
    {
        Aggresive,
        Attack,
        Coward,
        Panic,
        Neutral,
        Tower
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

    [Header("Tree")]
    private Node_Decision tree;
    private PKMNContext context;


    private void Awake()
    {
        //References
        pkmnRb = GetComponent<Rigidbody>();
        los = GetComponent<LineOfSight>();
        if (player != null)
            playerRb = player.GetComponent<Rigidbody>();

        //Wander restart
        wanderDirection = transform.forward;
        wanderTimer = 0f;
    }

    private void Start()
    {
        context = new PKMNContext { self = transform, player = player, los = los };

        //References to tree
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
        }
    }

    private void FixedUpdate()
    {
        if (state == State.Captured)
            return;

        if (isFleeing)
        {
            state = State.ToLake;
        }
        else
        {
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
        }
        Move(dir);
    }

    private void Move(Vector3 dir)
    {
        Vector3 hSpeed = dir * speed;
        float currentVSpeed = pkmnRb.linearVelocity.y;

        pkmnRb.linearVelocity = new Vector3(hSpeed.x, currentVSpeed, hSpeed.z);
        
        if (dir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void SetState(State state)
    {
        this.state = state;
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
        //Tras animación destruir. Todavía no por las dudas :P
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
}