using UnityEngine;
using System.Collections;

public class Robot2_KI : HealthHandler 
{
    public NavMeshAgent agent;
    public HoldPositionRigidbody holder;

    public bool Ragdolled = false;

    public Timer FindTargetTimer;
    public Timer StayRagdollTimer;

    public Transform BodyTransform;
    public Transform EyeTransform;
    public float SightRange = 10f;
    public LayerMask PlayerMask;
    public LayerMask SightMask;

	void Reset () 
    {
        FindTargetTimer.Reset();
        StayRagdollTimer.Reset();
	}

    public Transform Target = null;

    public override void Die()
    {
        alive = false;
        Ragdoll();
    }

	// Update is called once per frame
	void Update () 
    {
        if(Ragdolled)
        {
            if (!isDead && !StayRagdollTimer.Finished)
            {
                if (StayRagdollTimer.Update())
                {
                    holder.Ragdolled = true;
                }
                if (holder.Distance < 0.2f && holder.AngleDiff < 5)
                    UnRagdoll();
            }
            return;
        }

        if (FindTargetTimer.Update())
        {
            FindTarget();
            FindTargetTimer.Reset();
        }

        if (Target)
        {
            agent.SetDestination(Target.position);
        }
	}

    public void FindTarget()
    {
        if (Target)
        {
            if (Vector3.Distance(EyeTransform.position, Target.position) > SightRange)
                Target = null;
            RaycastHit hit;
            if (Target && Physics.Raycast(EyeTransform.position, (Target.position - EyeTransform.position).normalized, out hit, SightRange, SightMask))
            {
                if (hit.transform == Target)
                    return;
            }
        }
        Target = null;
        Collider[] colliders = Physics.OverlapSphere(EyeTransform.position, SightRange, PlayerMask);
        if (colliders.Length == 0)
            return;
        foreach (var target in colliders)
        {
            RaycastHit hit;
            if (Target && Physics.Raycast(EyeTransform.position, (Target.position - EyeTransform.position).normalized, out hit, SightRange, SightMask))
            {
                if (hit.collider == target)
                {
                    Target = target.transform;
                    return;
                }
            }
        }
    }

    public override void HitForce(float forceAmount, float minForceForRagdoll)
    {
        if (forceAmount > minForceForRagdoll)
            Ragdoll();
    }

    public void Ragdoll()
    {
        Ragdolled = true;
        holder.Ragdolled = true;
        agent.enabled = false;
    }

    public void UnRagdoll()
    {
        Ragdolled = false;
        holder.Ragdolled = false;
        NavMeshHit hit;
        if (NavMesh.FindClosestEdge(BodyTransform.position, out hit, 1 << NavMesh.GetNavMeshLayerFromName("Default")))
            agent.transform.position = hit.position;
        else
            agent.transform.position = BodyTransform.position;
        agent.enabled = true;
    }

    public float MaxAngle = 80;
    public float Speed = 2.0f;
    public float ndSpeed = 0f;

    public float StopDistance = 1.0f;

    void FixedUpdate()
    {
        if (Ragdolled)
            return;

        float delta = Game.EnemyFixedDelta;

        Vector3 targetVector = Vector3.zero;
        Vector3 currentVector = Vector3.zero;
        Vector3 crossResult = Vector3.zero;
        float cosAngle = 0f, turnAngle = 0f;

        targetVector = (holder.targetPosition.position - BodyTransform.position).normalized;
        currentVector = transform.forward;
        cosAngle = Vector3.Dot(currentVector, targetVector);
        crossResult = Vector3.Cross(currentVector, targetVector);
        crossResult.Normalize();
        turnAngle = Mathf.Acos(cosAngle);
        turnAngle = Mathf.Min(turnAngle * holder.LookSpring * delta, holder.MaxLookForce);
        turnAngle = turnAngle * Mathf.Rad2Deg;
        crossResult.x = 0f;
        crossResult.z = 0f;
        rigidbody.angularVelocity += crossResult * turnAngle;

        ndSpeed = holder.AngleDiff / 180f;
        if (holder.Distance > StopDistance)
            rigidbody.velocity += transform.forward * ndSpeed * Speed * delta;
    }
}
