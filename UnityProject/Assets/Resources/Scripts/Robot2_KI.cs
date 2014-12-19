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

    public bool NoTargetIsBase = true;

    void Awake()
    {
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        FindTargetTimer.Reset();
        StayRagdollTimer.Reset();

        laser.gameObject.SetActive(false);
    }

    public Transform Target = null;
    public bool CanSeeTarget = false;

    public Timer ShootCooldown;
    public Timer ShootTimer;

    public override void Die()
    {
        alive = false;
        Ragdoll();
    }

    public float ColorChangeSpeed = 2.0f;
    float ColorValue = 0f;

	// Update is called once per frame
	void Update () 
    {
        if(Ragdolled)
        {
            ResetLaser();
            if (!isDead && !StayRagdollTimer.Finished)
            {
                if (StayRagdollTimer.Update())
                {
                    holder.Ragdolled = false;
                }
                if (holder.Distance < 0.25f && holder.AngleDiff < 5)
                    UnRagdoll();
            }
            return;
        }

        if (FindTargetTimer.Update())
        {
            FindTarget();
            FindTargetTimer.Reset();
        }


        ShootCooldown.Update();
        if (Target)
        {
            agent.SetDestination(Target.position);
            if (CanSeeTarget)
            {
                Shoot();
            }
        }

        if (!Target || !CanSeeTarget)
        {
            ShootTimer.Reset();
            ResetLaser();
        }

        if (OutOfControl)
            agent.Stop();
        else
        {
            agent.Resume();
        }
            


        ColorValue = Mathf.Lerp(ColorValue, (CanSeeTarget ? 1f : 0f), Game.EnemyDelta * ColorChangeSpeed);
        EyeRenderer.materials[1].SetColor("_Color", Color.Lerp(NormalColor, AngryColor, ColorValue));
	}

    public LineRenderer laser;
    public Color NormalColor, AngryColor;

    public float DamagePerSecond = 10f;

    public Renderer EyeRenderer;

    public void Shoot()
    {
        if (ShootCooldown.Finished)
        {
            ShootTimer.Update();
            //Shoot
            laser.gameObject.SetActive(true);

            laser.SetPosition(0, EyeTransform.position);
            laser.SetPosition(1, EyeTransform.position + EyeTransform.forward * SightRange);

            Shooting = true;

            RaycastHit hit;
            if (Physics.Raycast(EyeTransform.position, EyeTransform.forward, out hit, SightRange, SightMask))
            {
                hit.transform.SendMessageUpwards("Damage", new vp_DamageInfo(DamagePerSecond * Game.EnemyDeltaTime, BodyTransform), SendMessageOptions.DontRequireReceiver);
            }

            if (ShootTimer.Finished)
            {
                ResetLaser();
            }
        }
    }

    public void ResetLaser()
    {
        laser.gameObject.SetActive(false);
        ShootCooldown.Reset();
        ShootTimer.Reset();
        Shooting = false;
    }

    public void FindTarget()
    {
        CanSeeTarget = false;
        if (Target)
        {
            if (Vector3.Distance(EyeTransform.position, Target.position) > SightRange)
                Target = null;
            RaycastHit hit;
            if (Target && Physics.Raycast(EyeTransform.position, (Target.position - EyeTransform.position).normalized, out hit, SightRange, SightMask))
            {
                if (hit.transform == Target)
                {
                    CanSeeTarget = true;
                    return;
                }
            }
        }
        Target = null;
        Collider[] colliders = Physics.OverlapSphere(EyeTransform.position, SightRange, PlayerMask);
        foreach (var target in colliders)
        {
            RaycastHit hit;
            if (Physics.Raycast(EyeTransform.position, (target.transform.position - EyeTransform.position).normalized, out hit, SightRange, SightMask))
            {
                if (hit.collider == target)
                {
                    Target = target.transform;
                    CanSeeTarget = true;
                    return;
                }
            }
        }
        if (NoTargetIsBase)
        {
            if (Base.Instance)
            {
                Target = Base.Instance.NavigationTarget;
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
        /*
        NavMeshHit hit;
        if (NavMesh.FindClosestEdge(BodyTransform.position, out hit, 1 << NavMesh.GetNavMeshLayerFromName("Default")))
            agent.Warp(hit.position);*/
        agent.Warp(BodyTransform.position);
        agent.enabled = true;
    }

    public float MaxAngle = 80;
    public float Speed = 2.0f;
    public float ndSpeed = 0f;

    public float StopDistance = 1.0f;

    public float OutOfControlDistance = 2.0f;
    public bool OutOfControl = false;

    public bool Shooting = false;

    void FixedUpdate()
    {
        if (Ragdolled)
            return;

        float delta = Game.EnemyFixedDelta;

        Vector3 targetVector = Vector3.zero;
        Vector3 currentVector = Vector3.zero;
        Vector3 crossResult = Vector3.zero;
        float cosAngle = 0f, turnAngle = 0f;

        if (holder.Distance > 0.5f || Target)
        {
            targetVector = (holder.targetPosition.position - BodyTransform.position).normalized;
            if (Target)
            {
                targetVector = (Target.position - BodyTransform.position).normalized;
            }
            currentVector = transform.forward;
            cosAngle = Vector3.Dot(currentVector, targetVector);
            crossResult = Vector3.Cross(currentVector, targetVector);
            crossResult.Normalize();
            turnAngle = Mathf.Acos(cosAngle);
            turnAngle = Mathf.Min(turnAngle * holder.LookSpring * delta, holder.MaxLookForce);
            turnAngle = turnAngle * Mathf.Rad2Deg;
            rigidbody.angularVelocity += crossResult * turnAngle;
        }

        OutOfControl = holder.Distance > OutOfControlDistance;

        if (holder.Distance > StopDistance)
        {
            if (!Target)
            {
                ndSpeed = holder.AngleDiff / 180f;
                rigidbody.velocity += transform.forward * ndSpeed * Speed * delta;
            }
            else
            {
                rigidbody.velocity += (holder.targetPosition.position - BodyTransform.position).normalized * Speed * delta;
            }
        }
    }
}
