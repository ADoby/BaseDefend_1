using UnityEngine;
using System.Collections;

public class Robot2_KI : Enemy
{
    public HoldPositionRigidbody holder;

    public bool Ragdolled = false;

    public Timer FindTargetTimer;
    public Timer StayRagdollTimer;

    public Transform BodyTransform;
    public Transform EyeTransform;
    public float SightRange = 10f;
    public LayerMask PlayerMask;
    public LayerMask SightMask;

    public Transform AgentTransform;
    public CharacterController MovingAgent;

    public float StopDistance = 1.0f;

    public bool NoTargetIsBase = true;

    private Vector3 CurrentSteeringTarget;

    public System.Collections.Generic.List<Vector3> CurrentPath = new System.Collections.Generic.List<Vector3>();
    private int currentPathIndex = 0;


    public float UpforceWhenDead = 2.0f;
    public float DeathYPosition = 100f;

    public bool IsLastWaypoint
    {
        get
        {
            return CurrentPath == null || CurrentPath.Count == 0 || (currentPathIndex == CurrentPath.Count);
        }
    }

    public void NewPath(System.Collections.Generic.List<Vector3> path)
    {
        CurrentPath = path;
        currentPathIndex = -1;
        NextWaypoint();
    }

    public void NextWaypoint()
    {
        if (IsLastWaypoint)
        {
            CurrentSteeringTarget = AgentTransform.position;
            return;
        }
        if (currentPathIndex < CurrentPath.Count - 1)
        {
            currentPathIndex++;
            CurrentSteeringTarget = CurrentPath[currentPathIndex];
        }
    }

    void Awake()
    {
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        FindTargetTimer.Reset();
        StayRagdollTimer.Reset();

        WaitAfterDeadTimer.Reset();
        laser.gameObject.SetActive(false);

        SetTarget(null, true);

        BodyTransform.position = transform.position;
        AgentTransform.position = transform.position;

        UnRagdoll();
    }

    public Transform Target = null;
    public bool CanSeeTarget = false;

    public Timer ShootCooldown;
    public Timer ShootTimer;

    public override void Die()
    {
        DropSettings.Drop(BodyTransform.position);
        alive = false;
        Ragdoll();
        Game.EnemyDied(this);
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
            else if (isDead)
            {
                WaitAfterDeadTimer.Update();
            }
            return;
        }

        if (FindTargetTimer.Update())
        {
            FindTarget();
            FindTargetTimer.Reset();
        }


        ShootCooldown.Update();
        if (UpdateTargetTimer.Update())
        {
            UpdateTargetTimer.Reset();
            if(Target)
                LevelManager.Instance.CurrentPart.PathFinder.FindPath(AgentTransform.position, Target.position, NewPath);
        }

        if (Target)
        {
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

        ColorValue = Mathf.Lerp(ColorValue, (CanSeeTarget ? 1f : 0f), Game.EnemyDelta * ColorChangeSpeed);
        EyeRenderer.materials[1].SetColor("_Color", Color.Lerp(NormalColor, AngryColor, ColorValue));
	}

    public LineRenderer laser;
    public Color NormalColor, AngryColor;

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
                hit.transform.SendMessageUpwards("Damage", new vp_DamageInfo(CurrentDamage * Game.EnemyDeltaTime, BodyTransform), SendMessageOptions.DontRequireReceiver);
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

    public void SetTarget(Transform target, bool force = false)
    {
        if (!force && target == Target)
            return;
        Target = target;
        if (Target)
        {
            LevelManager.Instance.CurrentPart.PathFinder.FindPath(AgentTransform.position, Target.position, NewPath);
        }
        else
        {
            CurrentSteeringTarget = AgentTransform.position;
            CurrentPath = null;
        }
    }

    public void FindTarget()
    {
        CanSeeTarget = false;
        if (Target)
        {
            if (Vector3.Distance(EyeTransform.position, Target.position) > SightRange)
                SetTarget(null);
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
        
        Collider[] colliders = Physics.OverlapSphere(EyeTransform.position, SightRange, PlayerMask);
        foreach (var target in colliders)
        {
            RaycastHit hit;
            if (Physics.Raycast(EyeTransform.position, (target.transform.position - EyeTransform.position).normalized, out hit, SightRange, SightMask))
            {
                if (hit.collider == target)
                {
                    SetTarget(target.transform);
                    CanSeeTarget = true;
                    return;
                }
            }
        }
        if (NoTargetIsBase)
        {
            if (Base.Instance)
            {
                SetTarget(Base.Instance.NavigationTarget);
                return;
            }
        }
        SetTarget(null);
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
    }

    public void UnRagdoll()
    {
        Ragdolled = false;
        holder.Ragdolled = false;
    }

    public float MaxAngle = 80;
    public float Speed = 2.0f;
    public float ndSpeed = 0f;

    public float HolderStopDistance = 1.0f;

    public float OutOfControlDistance = 2.0f;
    public bool OutOfControl = false;

    public bool Shooting = false;

    public Timer UpdateTargetTimer;

    public Timer WaitAfterDeadTimer;

    public override void Despawn()
    {
        Game.EnemyDespawned(this);
        base.Despawn();
    }

    void FixedUpdate()
    {
        float delta = Game.EnemyFixedDelta;
        if (Ragdolled)
        {
            if (isDead)
            {
                if (WaitAfterDeadTimer.Finished)
                {
                    BodyTransform.rigidbody.useGravity = false;
                    BodyTransform.rigidbody.AddForce(Vector3.up * UpforceWhenDead * delta);
                }
                if (BodyTransform.position.y > DeathYPosition)
                    Despawn();
            }
            return;
        }

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
            currentVector = BodyTransform.forward;
            cosAngle = Vector3.Dot(currentVector, targetVector);
            crossResult = Vector3.Cross(currentVector, targetVector);
            crossResult.Normalize();
            turnAngle = Mathf.Acos(cosAngle);
            turnAngle = Mathf.Min(turnAngle * holder.LookSpring * delta, holder.MaxLookForce);
            turnAngle = turnAngle * Mathf.Rad2Deg;
            BodyTransform.rigidbody.angularVelocity += crossResult * turnAngle;
        }

        OutOfControl = holder.Distance > OutOfControlDistance;

        if (holder.Distance > HolderStopDistance)
        {
            if (!Target)
            {
                ndSpeed = holder.AngleDiff / 180f;
                BodyTransform.rigidbody.velocity += BodyTransform.forward * ndSpeed * Speed * delta;
            }
            else
            {
                BodyTransform.rigidbody.velocity += (holder.targetPosition.position - BodyTransform.position).normalized * Speed * delta;
            }
       }

        #region Movement


        if (!IsLastWaypoint || Vector3.Distance(AgentTransform.position, CurrentSteeringTarget) > StopDistance)
            Move(delta);
        #endregion

        if (Vector3.Distance(AgentTransform.position, CurrentSteeringTarget) < StopDistance)
        {
            NextWaypoint();
        }
    }

    public Vector3 MoveDirection
    {
        get
        {
            return (CurrentSteeringTarget - AgentTransform.position).normalized;
        }
    }
    public float DestinationDistance
    {
        get
        {
            return Vector3.Distance(CurrentSteeringTarget, AgentTransform.position);
        }
    }
    public float TargetDistance
    {
        get
        {
            if (!Target)
                return 0f;
            return Vector3.Distance(Target.position, AgentTransform.position);
        }
    }
    public Vector3 TargetDirection
    {
        get
        {
            if (!Target)
                return Vector3.zero;
            return (Target.position - AgentTransform.position).normalized;
        }
    }

    #region Movement
    public float AgentMoveSpeed = 1.0f;

    public void Move(float delta)
    {
        MovingAgent.Move(MoveDirection * AgentMoveSpeed * delta);
        //MovingAgent.Move(-MovingAgent.velocity * 0.2f * delta);
        //MovingAgent.Move(Vector3.down * delta);
    }
    #endregion
}
