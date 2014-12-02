using UnityEngine;
using System.Collections;
using System.Linq;

public class RobotKI : Enemy 
{
	public bool PlayerInput = true;

	public bool CanMoveForward = true;
	public float ForwardSpeed = 2.0f;
	public bool CanMoveBackward = true;
	public float BackwardSpeed = 1.0f;
	public bool CanMoveSideward = true;
	public float SidewardSpeed = 2.0f;

	public bool CanRotate = true;
	public float RotationSpeed = 2.0f;

	public NavMeshAgent agent;

	public RobotBody Body;
	private Rigidbody rigidBody;
	public float SightRange = 10f;
	public float AttackRange = 5f;
	public LayerMask TargetLayer;
	public Transform Target;

	public Timer FindTargetTimer;
	public Timer UpdateTargetTimer;
	public Timer ShootTimer;
    public Timer WaitAfterDeadTimer;

	public RocketLauncher weapon;

    public override void Despawn()
    {
        agent.enabled = false;
        Game.EnemyDespawned(Game.EnemyType.ROBOT1);
        base.Despawn();
    }

	public bool HasTarget
	{
		get
		{
			return Target;
		}
	}
	public Vector3 Velocity
	{
		get
		{
			return rigidBody.velocity;
		}
		set
		{
			rigidBody.velocity = value;
		}
	}
	public Vector3 AngularVelocity
	{
		get
		{
			return rigidBody.angularVelocity;
		}
		set
		{
			rigidBody.angularVelocity = value;
		}
	}
	
	public Vector3 CurrentDestination
	{
		get
		{
			if (!agent)
				return Body.Position;
			return agent.steeringTarget;
		}
	}
	public Vector3 MoveDirection
	{
		get
		{
			return (CurrentDestination - Body.Position).normalized;
		}
	}
	public float DestinationDistance
	{
		get
		{
			return Vector3.Distance(CurrentDestination, Body.Position);
		}
	}
	public Vector3 TargetPosition
	{
		get
		{
			if (!Target)
				return Vector3.zero;
			return Target.position;
		}
	}
	public Vector3 TargetDirection
	{
		get
		{
			if (!Target)
				return Vector3.zero;
			return (Target.position - Body.Position).normalized;
		}
	}
	public float TargetDistance
	{
		get
		{
			if (!Target)
				return 0f;
			return Vector3.Distance(Target.position, Body.Position);
		}
	}
	public bool TargetInAttackRange
	{
		get
		{
			if (!Target)
				return false;
			return TargetDistance < AttackRange;
		}    
	}

	public override void Damage(vp_DamageInfo info)
	{
		base.Damage(info);
	}
	public override void Damage(float damage)
	{
		base.Damage(damage);
	}
	public override void Die()
	{
		//Despawn later
        Game.EnemyDied(Game.EnemyType.ROBOT1);
		Body.Ragdoll();
        WaitAfterDeadTimer.Reset();
	}

	public override void SetPoolName(string value)
	{
		base.SetPoolName(value);
	}

	// Use this for initialization
	void Awake ()
	{
		//We do this by force, so dont move guy
		agent.updatePosition = false;
		agent.updateRotation = false;

		rigidBody = Body.body.rigidbody;

		Body.PlayerControlled = PlayerInput;

		weapon.Owner = Body.body.transform;

        myCollider = GetComponentsInChildren<Transform>();
	}

	public override void Reset()
	{
        LastTargetPos = Base.Instance.NavigationTarget.position;
		base.Reset();
        Body.Reset();
        agent.enabled = true;
	}


	public Transform EyePosition;
	public LayerMask playerSight;
	public Vector3 LastTargetPos;
	public bool CanSeeTarget;

	void Update()
	{
        if (Body.Ragdolled || isDead)
        {
            WaitAfterDeadTimer.Update();
            return;
        }

        if (WayCheckTimer.Update())
        {
            SomeoneInMyWay = false;
            WayCheckTimer.Reset();
            RaycastHit hit;
            if (Physics.Raycast(EyePosition.position, WalkDirection, out hit, ObstacleDistance, ObstacleLayer))
            {
                if (!myCollider.Contains(hit.transform))
                {
                    SomeoneInMyWay = true;
                }
            }
        }

		if(FindTargetTimer.Update())
		{
			FindTargetTimer.Reset();
			FindTarget();
		}
		if (HasTarget)
		{
			CanSeeTarget = false;
			RaycastHit hit;
            if (Physics.Raycast(EyePosition.position, (Target.position - EyePosition.position).normalized, out hit, SightRange, playerSight))
			{
				if (hit.transform == Target)
				{
					CanSeeTarget = true;
					LastTargetPos = TargetPosition;
				}
			}
			if (!CanSeeTarget)
			{
				Target = null;
				TargetChanged();
			}
		}
		else
		{
			CanSeeTarget = false;
		}
		if (UpdateTargetTimer.Update())
		{
			UpdateTargetTimer.Reset();
			UpdateTarget();
		}
		if (ShootTimer.Update())
		{
			if (CanSeeTarget && TargetInAttackRange && !Body.Ragdolled)
			{

				if (weapon && weapon.Shoot())
				{
					ShootTimer.Reset();
				}
			}
		}
	}

	private void TargetChanged()
	{
		weapon.target = Target;
        UpdateTarget();
	}

    private bool TryTargeting(Collider coll)
    {
        if (coll == null || coll == default(Collider))
            return false;
        RaycastHit hit;
        if (Physics.Raycast(EyePosition.position, (coll.transform.position - EyePosition.position).normalized, out hit, SightRange, playerSight))
        {
            if (hit.collider == coll)
            {
                Target = coll.transform;
                LastTargetPos = TargetPosition;
                TargetChanged();
                return true;
            }
        }
        return false;
    }

	private void FindTarget()
	{
		Collider[] foundCollider = Physics.OverlapSphere(Body.Position, SightRange, TargetLayer);
        Collider player = foundCollider.FirstOrDefault(t => t.tag == "Player");
        if (TryTargeting(player))
            return;

        foreach (var coll in foundCollider)
        {
            if (TryTargeting(coll))
                return;
        }
        
	}

	public Timer RandomPositionTimer;

	private void UpdateTarget()
	{
        if (!CanSeeTarget || !TargetInAttackRange || (CanSeeTarget && !TargetInAttackRange))
        {
            agent.SetDestination(LastTargetPos);
        }
		else
		{
			if (RandomPositionTimer.Update())
			{
				RandomPositionTimer.Reset();

				Vector3 direction = Random.onUnitSphere;
                agent.SetDestination(Body.Position + direction);
			}
		}
	}


	private float dot = 0f;
	public void MovementValue(out float vertical, out float horizontal)
	{
		vertical = 0f;
		horizontal = 0f;

        if (CanSeeTarget && TargetDistance < 2.0f)
            return;

		Vector3 forward = Body.body.forward;
		forward.y = 0;
		Vector3 direction = MoveDirection;
		direction.y = 0;

		dot = Vector3.Dot(forward, direction);

        if(dot > 0)
            vertical = Mathf.Max(dot - 0.4f, 0.0f);
        else
            vertical = Mathf.Min(dot + 0.4f, 0.0f);

        forward = Body.body.right;
        forward.y = 0;
        dot = Vector3.Dot(forward, direction);
        //Move towards destination but sidewards
        if (dot > 0)
            horizontal = Mathf.Max(dot - 0.4f, 0.0f);
        else
            horizontal = Mathf.Min(dot + 0.4f, 0.0f);


		if (!CanSeeTarget)
		{
			//Move towards destination forwards
			//vertical = Mathf.Max(dot - 0.5f, 0.1f);
            horizontal *= 0.25f;
		}
		else
		{
            forward *= 0.25f;
		}
	}

	private float angle = 0f;
	public void RotationValue(out float rotation)
	{
		rotation = 0f;
		Vector3 forward = Body.body.forward;
		forward.y = 0;
		Vector3 direction = MoveDirection;
		if (CanSeeTarget)
			direction = TargetDirection;
		direction.y = 0;

		angle = Vector3.Angle(forward, direction);
		if (Vector3.Cross(forward, direction).y < 0) angle *= -1;
		angle /= 180;
		rotation = angle;
	}

	public float UpforceWhenDead = 2.0f;
	public float DeathYPosition = 100f;

	// Update is called once per frame
	void FixedUpdate ()
	{
		float delta = Time.fixedDeltaTime / Game.DefaultFixedTime;
		if (Body.Ragdolled)
		{
            if (isDead)
			{
                if (WaitAfterDeadTimer.Finished)
                {
                    Body.body.rigidbody.useGravity = false;
                    Body.body.rigidbody.AddForce(Vector3.up * UpforceWhenDead * delta);
                }
                if (Body.Position.y > DeathYPosition)
					Despawn();
			}
			else
			{
				Body.GetUp(delta);
			}
			return;
		}


		#region Movement
		float verticalInput = 0;
		float horizontalInput = 0;

		if (PlayerInput)
		{
			verticalInput = Input.GetAxis("Vertical");
			horizontalInput = Input.GetAxis("Horizontal");
		}
		else
		{
			//AI controlled input
			MovementValue(out verticalInput, out horizontalInput);
		}

		Move(verticalInput, horizontalInput, delta);
		#endregion
		

		#region Rotation
		float rotationInput = 0;

		if (PlayerInput)
		{
			rotationInput = Input.GetAxis("Horizontal");
		}
		else
		{
			//AI controlled input
			RotationValue(out rotationInput);
		}

		Rotate(rotationInput, delta);
		#endregion

		
	}


    public LayerMask ObstacleLayer;
    public float ObstacleDistance = 1.0f;

    public Transform[] myCollider;

    public Timer WayCheckTimer;
    public bool SomeoneInMyWay = false;
    public Vector3 WalkDirection = Vector3.zero;

	public void Move(float verticalInput, float horizontalInput, float delta)
	{
		Vector3 inputDirection = Vector3.zero;

		if (verticalInput > 0)
		{
			if (CanMoveForward)
				verticalInput *= ForwardSpeed;
			else
				verticalInput = 0;
		}
		else
		{
			if (CanMoveBackward)
				verticalInput *= BackwardSpeed;
			else
				verticalInput = 0;
		}
		inputDirection.z = verticalInput;

		if (CanMoveSideward)
			horizontalInput *= SidewardSpeed;
		else
			horizontalInput = 0;
		inputDirection.x = horizontalInput;

        WalkDirection = inputDirection;

		inputDirection = Body.body.TransformDirection(inputDirection);

		if (Body.CanMove)
			Velocity += inputDirection;
		
	}
	public void Rotate(float rotationInput, float delta)
	{
		if (CanRotate)
			rotationInput *= RotationSpeed;
		else
			rotationInput = 0;

		if (Body.CanMove)
			AngularVelocity += rotationInput * Vector3.up;
	}
}
