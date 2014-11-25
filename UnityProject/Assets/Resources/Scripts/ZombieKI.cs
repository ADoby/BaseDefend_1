using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ZombieKI : HealthHandler
{
    NavMeshAgent agent;
    public Animator animator;

    private Vector3 targetPos;
    public void SetTargetPos(Vector3 pos)
    {
        targetPos = pos;
    }

    public bool canSeeTarget { get; set; }

    private Vector3 currentMovement = Vector3.zero;
    private Vector3 currentFallSpeed = Vector3.zero;
    private float currentRotation = 0;

    [SerializeField]
    private float diffY;
    [SerializeField]
    private float maxDiffAngle = 0.2f;

    public float RotateSpeed = 2.0f;
    public float MoveSpeed = 10.0f;
    public float LookRange = 10.0f;

    [SerializeField]
    private float maxRotateSpeed = 2.0f;
    [SerializeField]
    private float rotateChange = 10.0f;
    [SerializeField]
    private float rotateBreak = 20.0f;

    [SerializeField]
    private float maxMoveSpeed = 10.0f;
    [SerializeField]
    private float speedChange = 5.0f;
    [SerializeField]
    private float speedBrake = 10.0f;

    public float ragdollTime = 2.0f;
    private float ragdollTimer = 0.0f;
    private bool ragdolled = false;

    public Rigidbody root;

    public float PathUpdateTime = 0.2f;

    public float minHealth = 10.0f;
    public float maxHealth = 15.0f;

    public float minSpeed = 1.0f;
    public float maxSpeed = 2.0f;

    public Sight sightScript;


    // Use this for initialization
    void Awake()
    {
        Reset();
    }

    public RagdollHelper animatedRagdollCharakter;

    public void Randomize()
    {
        health = Random.Range(minHealth, maxHealth);

    }

    public override void Reset()
    {
        base.Reset();
        if(!animatedRagdollCharakter)
            animatedRagdollCharakter = GetComponent<RagdollHelper>();

        player = FindObjectOfType<vp_PlayerDamageHandler>();

        Randomize();

        animatedRagdollCharakter.ragdolled = false;
        ragdolled = false;
        currentUpwardsVelocity = 0.0f;
        animatedRagdollCharakter.setKinematic(true);

        if(!agent)
            agent = GetComponent<NavMeshAgent>();

        if(!animator)
            animator = GetComponent<Animator>();

        

        //agent.enabled = true;
        animator.enabled = true;

        agent.updateRotation = false;
        targetPos = transform.position;

        animator.SetTrigger("WakeUp");

        StartCoroutine(UpdatePath());
    }

    public override void Damage(vp_DamageInfo info)
    {
        base.Damage(info);
        if (isDead)
            return;
        if (isDead)
        {
            ResetDeathRaise();
            animatedRagdollCharakter.ragdolled = true;
        }
    }
    public override void Damage(float damage)
    {
        base.Damage(damage);
    }

    public void ResetDeathRaise()
    {
        currentUpwardsVelocity = 0.0f;
        deathRagdollTimer = 0f;
    }

    public override void HitForce(float forceAmount, float minForceForRagdoll)
    {

        if(isDead)
            ResetDeathRaise();

        if (forceAmount > minForceForRagdoll * Mathf.Clamp01(health / maxHealth))
        {
            animatedRagdollCharakter.ragdolled = true;
            ragdolled = true;
            ragdollTimer = ragdollTime;
        }
    }

    public float randomMoveDistance = 4.0f;

    public float randomMoveTimer = 0f;
    public float minRandomMoveTime = 3f, maxRandomMoveTime = 6f;

    private void GetRandomPosition()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-randomMoveDistance, randomMoveDistance), 0, Random.Range(-randomMoveDistance, randomMoveDistance));
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position+randomDirection, out hit, randomMoveDistance, 1 << NavMesh.GetNavMeshLayerFromName("Default")))
        {
            targetPos = hit.position;
        }

        randomMoveTimer = Random.Range(minRandomMoveTime, maxRandomMoveTime);
    }

    IEnumerator UpdatePath()
    {
        if (!ragdolled && agent)
        {
            if (agent.enabled)
            {
                if (!canSeeTarget)
                {
                    if (agent.remainingDistance < breakDistance && randomMoveTimer <= 0f)
                        GetRandomPosition();
                }
                agent.SetDestination(targetPos);
            }
        }

        yield return new WaitForSeconds(PathUpdateTime);
        if(!isDead)
            StartCoroutine(UpdatePath());
    }

    public void WalkTowards(Vector3 pos)
    {
        if (Vector3.Distance(transform.position, pos) > 0.1f && Mathf.Abs(diffY) < maxDiffAngle && agent.remainingDistance > breakDistance)
        {
            currentMovement = Vector3.Lerp(currentMovement, (pos - transform.position).normalized * maxMoveSpeed * (maxDiffAngle - Mathf.Abs(diffY)), Time.deltaTime * speedChange);
        }
        else
        {
            currentMovement = Vector3.Lerp(currentMovement, Vector3.zero, Time.deltaTime * speedBrake);
        }

        animator.SetFloat("Forward", currentMovement.magnitude);
    }

    public float minDifferenceForRotating = 0.2f;

    public void RotateTowards(Vector3 pos)
    {
        Vector3 localTarget = transform.InverseTransformPoint(pos);
        diffY = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        diffY = Mathf.Clamp(diffY / 180, -1, 1);

        if (Mathf.Abs(diffY) > minDifferenceForRotating)
        {
            currentRotation = Mathf.Lerp(currentRotation, diffY * maxRotateSpeed, Time.deltaTime * rotateChange);
        }
        else
        {
            currentRotation = Mathf.Lerp(currentRotation, 0, Time.deltaTime * rotateBreak);
        }
        

        animator.SetFloat("Turn", currentRotation);
    }

    public float breakDistance = 1.0f;

    public float deathUpwardsForce = 1.0f;
    public float maxForceUpwards = 2.0f;

    public override void SetPoolName(string value)
    {
        base.SetPoolName(value);
    }

    public float currentUpwardsVelMultiply = 1.0f;
    public float currentUpwardsVelocity = 0.0f;

    public float deathRagdollTime = 1.0f;
    private float deathRagdollTimer = 0f;
    
    public vp_PlayerDamageHandler player;

    public float AttackCooldown = 2.0f;
    private float AttackTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < breakDistance)
        {
            randomMoveTimer -= Time.deltaTime;
        }
        AttackTimer -= Time.deltaTime;

        if (player != null && ragdolled == false && !isDead)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < 1.0f)
            {
                if (AttackTimer <= 0)
                {
                    AttackTimer = AttackCooldown;

                    player.Damage(new vp_DamageInfo(5f, transform));
                }
            }
        }
        

        if (ragdolled)
        {
            if (Mathf.Abs(root.velocity.magnitude) < 0.1f)
            {
                ragdollTimer -= Time.deltaTime;
                if (ragdollTimer <= 0)
                {
                    ragdolled = false;
                    if (!isDead)
                    {
                        animatedRagdollCharakter.ragdolled = false;

                        List<Transform> childs = new List<Transform>();
                        foreach (Transform child in transform)
                        {
                            child.parent = null;
                            childs.Add(child);
                        }
                        transform.position = root.position;

                        foreach (var item in childs)
                        {
                            item.parent = transform;
                        }

                        agent.SetDestination(transform.position);
                    }
                }
            }
            else
            {
                //As long as the rigidbody has force (probably falling) we cant get up
                ragdollTimer = ragdollTime;
            }
        }
        else
        {
            if (isDead)
            {
                deathRagdollTimer += Time.deltaTime;
                if (deathRagdollTimer > deathRagdollTime)
                {
                    currentUpwardsVelocity = Mathf.Lerp(currentUpwardsVelocity, maxForceUpwards, Time.deltaTime * deathUpwardsForce + Time.deltaTime * (currentUpwardsVelocity * currentUpwardsVelMultiply));

                    if (!root.isKinematic)
                        root.velocity = new Vector3(root.velocity.x, currentUpwardsVelocity, root.velocity.z);

                    if (root.transform.position.y >= 100)
                    {
                        Despawn();
                    }
                }

                
            }
            else
            {
                RotateTowards(agent.steeringTarget);

                //As Long as we are not near our last target, walk towards next target
                WalkTowards(agent.steeringTarget);

                agent.velocity = Vector3.zero;
            }
        }
    }
}
