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

    public RobotBody Body;
    private Rigidbody rigidBody;
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

	// Use this for initialization
	void Start () 
    {
        rigidBody = Body.body.rigidbody;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
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
