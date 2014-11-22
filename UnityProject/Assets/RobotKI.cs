using UnityEngine;
using System.Collections;

public class RobotKI : MonoBehaviour 
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

	// Use this for initialization
	void Awake () 
    {
        //We do this by force, so dont move guy
        agent.updatePosition = false;
        agent.updateRotation = false;

        rigidBody = Body.body.rigidbody;

        Body.PlayerControlled = PlayerInput;
	}

    void Update()
    {
        if (Body.Ragdolled)
            return;

        if(FindTargetTimer.Update())
        {
            FindTargetTimer.Reset();
            FindTarget();
        }
        if (UpdateTargetTimer.Update())
        {
            UpdateTargetTimer.Reset();
            UpdateTarget();
        }
    }

    private void FindTarget()
    {
        Collider[] foundCollider = Physics.OverlapSphere(Body.Position, SightRange, TargetLayer);
        foreach (var coll in foundCollider)
        {
            Target = coll.transform;
            UpdateTarget();
            return;
        }
    }

    private void UpdateTarget()
    {
        if(!TargetInAttackRange)
            agent.SetDestination(TargetPosition);
        else
            agent.SetDestination(Body.Position);
    }

    public float dot = 0f;
    public void MovementValue(out float vertical, out float horizontal)
    {
        vertical = 0f;
        horizontal = 0f;

        Vector3 forward = Body.body.forward;
        forward.y = 0;
        Vector3 direction = MoveDirection;
        direction.y = 0;

        dot = Vector3.Dot(forward, direction);

        if (!TargetInAttackRange)
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

    public float angle = 0f;
    public void RotationValue(out float rotation)
    {
        rotation = 0f;
        Vector3 forward = Body.body.forward;
        forward.y = 0;
        Vector3 direction = MoveDirection;
        if (TargetInAttackRange)
            direction = TargetDirection;
        direction.y = 0;

        angle = Vector3.Angle(forward, direction);
        if (Vector3.Cross(forward, direction).y < 0) angle *= -1;
        angle /= 180;
        rotation = angle;
    }

    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        if (Body.Ragdolled)
            return;

        float delta = Time.fixedDeltaTime / Game.DefaultFixedTime;

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
