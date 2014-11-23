using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieKI1 : HealthHandler
{
    public NavMeshAgent agent;
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

    private float diffY;
    private float maxDiffAngle = 0.2f;

    public float RotateSpeed = 2.0f;
    public float MoveSpeed = 10.0f;
    public float LookRange = 10.0f;

    private float maxRotateSpeed = 2.0f;
    private float rotateChange = 10.0f;
    private float rotateBreak = 20.0f;

    private float maxMoveSpeed = 10.0f;
    private float speedChange = 5.0f;
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

    public override void Reset()
    {
        base.Reset();

        health = Random.Range(minHealth, maxHealth);

        float random = Random.Range(minSpeed, maxSpeed);

        maxMoveSpeed = random * MoveSpeed;
        speedChange = maxMoveSpeed * 0.5f;
        speedBrake = maxMoveSpeed;

        maxRotateSpeed = random * RotateSpeed;
        rotateChange = maxRotateSpeed * 5.0f;
        rotateBreak = maxRotateSpeed * 10.0f;

        maxDiffAngle = random / 10.0f;

        if (sightScript)
        {
            sightScript.distance = random * LookRange;
            sightScript.sightTime = random;
        }

        ragdolled = false;
        currentUpwardsVelocity = 0.0f;

        if(!agent)
            agent = GetComponent<NavMeshAgent>();

        if(!animator)
            animator = GetComponent<Animator>();

        agent.enabled = true;
        animator.enabled = true;

        agent.updateRotation = false;
        targetPos = transform.position;

        StartCoroutine(UpdatePath());
    }

    public override void Damage(vp_DamageInfo info)
    {
        base.Damage(info);
        currentUpwardsVelocity = 0.0f;
    }


    public AnimatedRagdoll ragdollScript;

    public override void HitForce(float forceAmount, float minForceForRagdoll)
    {
        if (forceAmount > minForceForRagdoll)
        {
            ragdollScript.Ragdoll(true);

            ragdolled = true;
            ragdollTimer = ragdollTime;
            agent.enabled = false;
        }
    }

    public float randomMoveDistance = 4.0f;

    private void GetRandomPosition()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-randomMoveDistance, randomMoveDistance), 0, Random.Range(-randomMoveDistance, randomMoveDistance));
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position+randomDirection, out hit, randomMoveDistance, 1 << NavMesh.GetNavMeshLayerFromName("Default")))
        {
            targetPos = hit.position;
        }
    }

    IEnumerator UpdatePath()
    {
        if (!ragdolled && agent)
        {
            if (agent.enabled)
            {
                if (!canSeeTarget)
                {
                    if(agent.remainingDistance < breakDistance)
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

    public float currentUpwardsVelMultiply = 1.0f;
    public float currentUpwardsVelocity = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (ragdolled)
        {
            if (Mathf.Abs(root.velocity.magnitude) < 0.1f)
            {
                ragdollTimer -= Time.deltaTime;
                if (ragdollTimer <= 0)
                {
                    ragdolled = false;
                    ragdollScript.Ragdoll(false);

                    if (!isDead)
                    {

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

                        agent.enabled = true;
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
                currentUpwardsVelocity = Mathf.Lerp(currentUpwardsVelocity, maxForceUpwards, Time.deltaTime * deathUpwardsForce + Time.deltaTime * (currentUpwardsVelocity * currentUpwardsVelMultiply));
                
                if(!root.isKinematic)
                    root.velocity = new Vector3(root.velocity.x, currentUpwardsVelocity, root.velocity.z);

                if (root.transform.position.y >= 100)
                {
                    Despawn();
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
