using UnityEngine;
using System.Collections;

public class RobotKI : HealthHandler 
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

	public RocketLauncher weapon;

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
		Body.Ragdoll();
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
	}

	public override void Reset()
	{
		base.Reset();
	}


	public Transform EyePosition;
	public LayerMask playerSight;
	public Vector3 LastTargetPos;
	public bool CanSeeTarget;

	void Update()
	{
		if (Body.Ragdolled || isDead)
			return;

		if(FindTargetTimer.Update())
		{
			FindTargetTimer.Reset();
			FindTarget();
		}
		if (HasTarget)
		{
			CanSeeTarget = false;
			RaycastHit hit;
			if (Physics.Raycast(EyePosition.position, TargetDirection, out hit, SightRange, playerSight))
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
			if (CanSeeTarget && TargetInAttackRange)
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

	private void FindTarget()
	{
		Collider[] foundCollider = Physics.OverlapSphere(Body.Position, SightRange, TargetLayer);
		foreach (var coll in foundCollider)
		{
			RaycastHit hit;
			if (Physics.Raycast(EyePosition.position, coll.transform.position- EyePosition.position, out hit, SightRange, playerSight))
			{
				Target = coll.transform;
				TargetChanged();
				return;
			}
		}
	}

	public Timer RandomPositionTimer;

	private void UpdateTarget()
	{
		if(!TargetInAttackRange || !CanSeeTarget)
			agent.SetDestination(LastTargetPos);
		else
		{
			if (RandomPositionTimer.Update())
			{
				RandomPositionTimer.Reset();

				Vector3 direction = Random.onUnitSphere;
				NavMeshHit hit;
				if (NavMesh.SamplePosition(Body.Position + direction, out hit, 2f, NavMesh.GetNavMeshLayerFromName("Default")))
				{
					agent.SetDestination(hit.position);
				}
				else
				{
					agent.SetDestination(Body.Position);
				}
			}
		}
	}

	private float dot = 0f;
	public void MovementValue(out float vertical, out float horizontal)
	{
		vertical = 0f;
		horizontal = 0f;

		Vector3 forward = Body.body.forward;
		forward.y = 0;
		Vector3 direction = MoveDirection;
		direction.y = 0;

		dot = Vector3.Dot(forward, direction);

		if (!CanSeeTarget)
		{
			//Move towards destination forwards
			vertical = Mathf.Max(dot - 0.7f, 0);
		}
		else
		{
			//Move towards destination but sidewards
			horizontal = dot;
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
				Body.body.rigidbody.useGravity = false;
				Body.body.rigidbody.AddForce(Vector3.up * UpforceWhenDead * delta);
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
