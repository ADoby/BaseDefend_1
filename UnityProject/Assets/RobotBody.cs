using UnityEngine;
using System.Collections;



[System.Serializable]
public class Leg
{
    private const float PI = 3.1415f;
    public string Name;

    [System.Serializable]
    public struct Targets
    {
        public Transform Root;
        public Transform Foot;
        public Transform FootIK;
        public Transform DefaultPosition;
        public SimpleIKSolver ik;
    }

    public void SetRagdoll(bool value)
    {
        if (!value)
        {
            targets.ik.ResetJoints();
            Reset();
        }
        targets.ik.IsActive = !value;
    }

    private RobotBody owner;
    public RobotBody Owner
    {
        get
        {
            return owner;
        }
        set
        {
            owner = value;
        }
    }

    public Targets targets;
    public Transform Root
    {
        get
        {
            return targets.Root;
        }
    }
    public Transform Foot
    {
        get
        {
            return targets.Foot;
        }
    }
    public Transform FootIK
    {
        get
        {
            return targets.FootIK;
        }
    }
    public Transform DefaultPosition
    {
        get
        {
            return targets.DefaultPosition;
        }
    }
    public float MaxFeetDistance = 1.2f;
    public Timer TransitionTimer;
    public float TransitionYDiff = 0.5f;
    public float TransitionDamping = 1.0f;

    public Vector4 moveRestriction;

    //DEBUG
    private float CurrentDistance = 0f;
    private bool needsNewPosition = false;
    public bool SpecialPosition = true;

    private Vector3 LastPosition;
    private Vector3 WantedPosition;
    private float procentage = 0f;

    public Timer NewPositionTimer;

    public void Start()
    {
        Reset();
    }

    private bool moving = true;
    private bool wasGrounded = false;

    public void Reset()
    {
        LastPosition = Foot.position;
        WantedPosition = LastPosition;
        TransitionTimer.Reset();
        moving = true;
        wasGrounded = false;
    }

    private bool NeedsNewPosition()
    {
        Vector3 min = new Vector3(moveRestriction.x, 0, moveRestriction.z);
        Vector3 max = new Vector3(moveRestriction.y, 0, moveRestriction.w);

        Vector3 position = Owner.body.InverseTransformPoint(FootIK.position);

        if (position.x < min.x || position.x > max.x)
            return true;
        if (position.z < min.z || position.z > max.z)
            return true;

        return false;
    }

    private bool GetGround(Vector3 position, out Vector3 hitPoint)
    {
        RaycastHit hit;
        Vector3 origin = position;
        origin.y = Owner.body.position.y + Owner.SlopeStairHeight;
        if (Physics.Raycast(origin, Vector3.down, out hit, MaxFeetDistance + Owner.SlopeStairHeight, Owner.FootWalkLayerMask))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = position;
        return false;
    }

    public void DebugRestrictionArea()
    {
        Vector3 min = new Vector3(moveRestriction.x, 0, moveRestriction.z);
        Vector3 max = new Vector3(moveRestriction.y, 0, moveRestriction.w);
        Vector3 pos1 = min;
        Vector3 pos2 = new Vector3(min.x, min.y, max.z);
        Vector3 pos3 = max;
        Vector3 pos4 = new Vector3(max.x, min.y, min.z);

        pos1.y -= Owner.YDistanceToFeet;
        pos2.y -= Owner.YDistanceToFeet;
        pos3.y -= Owner.YDistanceToFeet;
        pos4.y -= Owner.YDistanceToFeet;

        pos1 = Owner.body.TransformPoint(pos1);
        pos2 = Owner.body.TransformPoint(pos2);
        pos3 = Owner.body.TransformPoint(pos3);
        pos4 = Owner.body.TransformPoint(pos4);
        

        Debug.DrawLine(pos1, pos2, Color.red);
        Debug.DrawLine(pos2, pos3, Color.red);
        Debug.DrawLine(pos3, pos4, Color.red);
        Debug.DrawLine(pos4, pos1, Color.red);

        Vector3 centerOfFootPlacement = Vector3.zero;
        centerOfFootPlacement.x = min.x + (max.x - min.x)/2;
        centerOfFootPlacement.z = min.z + (max.z - min.z)/2;
        centerOfFootPlacement.y -= Owner.YDistanceToFeet;
        centerOfFootPlacement = Owner.body.TransformPoint(centerOfFootPlacement);

        Debug.DrawRay(centerOfFootPlacement, Vector3.up, Color.green);
    }

    

    public void Update()
    {
        NewPositionTimer.Update();

        //Stick on ground
        if (!GetGround(WantedPosition, out WantedPosition))
        {
            GetGround(DefaultPosition.position, out WantedPosition);
            SpecialPosition = false;
        }
        else
            SpecialPosition = true;

        //Animate to next wanted position
        TransitionTimer.Update();
        procentage = TransitionTimer.Procentage;
        float sinVal = Mathf.Sin(procentage * PI);

        Vector3 wantedPos = procentage < 0.5f ? LastPosition : WantedPosition;
        wantedPos.y += sinVal * TransitionYDiff;

        //diff = diff.normalized * Mathf.Min(TransitionDamping, distanceToTarget);
        FootIK.position = Vector3.Lerp(FootIK.position, wantedPos, TransitionTimer.Procentage);
        
        moving = !TransitionTimer.Finished;
        
        if (OnGround && !wasGrounded)
        {
            Owner.FootStepOnGround();
            wasGrounded = true;
        }

        needsNewPosition = NeedsNewPosition();
        //Check if body moves and try to find new position for footIK
        if (!moving && needsNewPosition)
        {
            if (!Owner.CanLoseFoot)
                return;

            Vector3 newPos = Vector3.zero;
            Vector3 DiffToCenter = Vector3.zero;
            Vector3 centerOfFootPlacement = Vector3.zero;

            Vector3 min = new Vector3(moveRestriction.x, 0, moveRestriction.z);
            Vector3 max = new Vector3(moveRestriction.y, 0, moveRestriction.w);

            min.x += 0.1f;
            min.z += 0.1f;
            max.x -= 0.1f;
            max.z -= 0.1f;

            centerOfFootPlacement.x = min.x + (max.x - min.x) / 2;
            centerOfFootPlacement.z = min.z + (max.z - min.z) / 2;
            centerOfFootPlacement.y -= Owner.YDistanceToFeet;

            Vector3 moveDirection = Owner.body.InverseTransformDirection(Owner.body.rigidbody.velocity);
            if (moveDirection.magnitude <= 1f)
            {
                moveDirection = centerOfFootPlacement - Owner.body.InverseTransformPoint(FootIK.position);
            }
            moveDirection.y = 0f;
            moveDirection.Normalize();

            newPos = centerOfFootPlacement + moveDirection * 100f;

            newPos.x = Mathf.Clamp(newPos.x, min.x, max.x);
            newPos.z = Mathf.Clamp(newPos.z, min.z, max.z);
            newPos.y = centerOfFootPlacement.y;
            newPos = Owner.body.TransformPoint(newPos);

            LastPosition = WantedPosition;
            WantedPosition = newPos;
            TransitionTimer.Reset();
            moving = true;

            if (wasGrounded)
                Owner.LoseFoot();
            wasGrounded = false;
        }
    }

    public float MaxDistanceToTarget = 0.15f;

    public bool OnTrack
    {
        get
        {
            return Vector3.Distance(FootIK.position, Foot.position) < MaxDistanceToTarget;
        }
    }

    public bool OnGround
    {
        get
        {
            return TransitionTimer.Finished && SpecialPosition && OnTrack;
        }
    }
}

[ExecuteInEditMode]
public class RobotBody : MonoBehaviour 
{
    public LayerMask FootWalkLayerMask;
    public Leg[] legs = {};

    public int feetOnGround = 0;

    void Start()
    {
        Reset();
    }

	// Use this for initialization
	void Reset () 
    {
        feetOnGround = 0;
        foreach (var item in legs)
        {
            item.Owner = this;
            item.Reset();
        }
	}

    public Timer GetUpTimer;
    public Timer StayRagdollTimer;

    public Timer FootLoseTimer;
    public Timer FootGroundedTimer;

    public Timer RagdollAfterNoGroundTimer;
    
    public float SlopeStairHeight = 1.0f;

    public bool CanLoseFoot
    {
        get
        {
            return feetOnGround >= 2;
        }
    }
    public void LoseFoot()
    {
        feetOnGround--;
        FootLoseTimer.Reset();
    }
    public void FootStepOnGround()
    {
        feetOnGround++;
        FootGroundedTimer.Reset();
    }

    public bool DebugON = true;
    public bool RunInEditor = true;

    public Transform body;

    [Range(0f, 1f)]
    public float VelocityDamping = 0.2f;
    [Range(0f, 1.0f)]
    public float RotateVelocityDamping = 0.2f;

    public float GroundDistance = 0.75f;
    [Range(0f, 10f)]
    public float HeightSpring = 5.0f;
    [Range(0f, 1f)]
    public float HeightDamping = 1.0f;

    [Range(0f, 0.25f)]
    public float RotationSpring = 3.0f;
    public float RotateDamping = 1.0f;
    
    public float YDistanceToFeet = 1.0f;
    public float FeetSpring = 5.0f;
    public float FeetDamping = 1.0f;

    public float Gravity = -10f;

	// Update is called once per frame
	void Update ()
    {
        if (Ragdolled)
            return;

        FootLoseTimer.Update();
        FootGroundedTimer.Update();

        float heightestYFoot = 0f;
        for (int i = 0; i < legs.Length; i++)
        {
            Leg leg = legs[i];

            if(Application.isPlaying)
                leg.Update();

            float y = leg.Foot.position.y;

            if (i == 0 || y > heightestYFoot)
                heightestYFoot = y;

            if (DebugON)
                leg.DebugRestrictionArea();
        }

        if (CanMove)
        {
            RagdollAfterNoGroundTimer.Reset();
        }
        else
        {
            if (RagdollAfterNoGroundTimer.Update())
                Ragdoll();
        }

	}

    public bool CanMove
    {
        get
        {
            return (feetOnGround >= 2);
        }
    }

    public Vector3 Position
    {
        get
        {
            return body.position;
        }
    }

    public bool grounded;
    public bool PlayerControlled = true;
    public float GroundCheckDistance = 1.25f;

    public bool Ragdolled = false;
    public float MaxDiffBodyAngle = 40f;
    public bool FeetYToBodyY = false;

    public GameObject PlayerMovementCollider;

    public float DiffAngle = 0f;

    public void Ragdoll()
    {
        body.rigidbody.useGravity = true;

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var item in rigidbodies)
        {
            if (item == body.rigidbody)
                continue;

            ConfigurableJoint joint = item.GetComponent<ConfigurableJoint>();
            if(joint)
            {
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
            }
            item.collider.isTrigger = false;
            item.isKinematic = false;
            item.useGravity = true;
        }
        if (PlayerMovementCollider && PlayerControlled)
            PlayerMovementCollider.SetActive(false);

        foreach (var leg in legs)
        {
            leg.SetRagdoll(true);
        }

        Ragdolled = true;

        StayRagdollTimer.Reset();
        GetUpTimer.Reset();
        StartGetUpPosition = body.position;
        RagdollAfterNoGroundTimer.Reset();
    }

    private Vector3 StartGetUpPosition = Vector3.zero;

    public float GetUpSpring = 2.0f;
    public float GetUpDamp = 0.5f;
    public float GetUpRotationSpring = 2.0f;
    public float GetUpRotationDamp = 0.5f;

    public void GetUp(float delta)
    {
        if (!StayRagdollTimer.Update())
            return;

        body.rigidbody.useGravity = false;

        if (GetUpTimer.Update())
        {
            UnRagdoll();
            return;
        }

        //Update Position and Rotation
        Vector3 currentVector = body.up;
        Vector3 targetVector = Vector3.up;
        Vector3 wantedPosition = StartGetUpPosition + Vector3.up;


        RaycastHit hit;
        if (Physics.Raycast(body.position, -body.up, out hit, GroundCheckDistance, FootWalkLayerMask))
        {
            wantedPosition = hit.point + Vector3.up;
            //targetVector = hit.normal;
        }

        body.rigidbody.velocity += (wantedPosition - body.position) * GetUpSpring * delta;

        body.rigidbody.velocity -= body.rigidbody.velocity * GetUpDamp * delta;

        float cosAngle;
        Vector3 crossResult;
        float turnAngle;


        cosAngle = Vector3.Dot(currentVector, targetVector);

        crossResult = Vector3.Cross(currentVector, targetVector);
        crossResult.Normalize();

        turnAngle = Mathf.Acos(cosAngle);
        turnAngle = Mathf.Min(turnAngle, RotateDamping);
        turnAngle = turnAngle * Mathf.Rad2Deg;

        body.rigidbody.angularVelocity += crossResult * turnAngle * GetUpRotationSpring * delta;

        body.rigidbody.angularVelocity -= body.rigidbody.angularVelocity * GetUpRotationDamp * delta;
    }

    public void UnRagdoll()
    {
        body.rigidbody.useGravity = false;

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var item in rigidbodies)
        {
            if (item == body.rigidbody)
                continue;

            ConfigurableJoint joint = item.GetComponent<ConfigurableJoint>();
            if (joint)
            {
                joint.xMotion = ConfigurableJointMotion.Free;
                joint.yMotion = ConfigurableJointMotion.Free;
                joint.zMotion = ConfigurableJointMotion.Free;
            }
            item.collider.isTrigger = true;
            item.isKinematic = true;
            item.useGravity = false;
        }
        if (PlayerMovementCollider && PlayerControlled)
            PlayerMovementCollider.SetActive(true);

        foreach (var leg in legs)
        {
            leg.SetRagdoll(false);
        }

        Ragdolled = false;
        Reset();
    }

    void FixedUpdate()
    {
        if (!Application.isPlaying)
            return;

        float delta = Time.fixedDeltaTime / Game.DefaultFixedTime;
        if (Ragdolled)
        {
            GetUp(delta);
            return;
        }

        //Based on ground
        float currentHeightDamping = HeightDamping;
        float currentVelocityDamping = VelocityDamping;


        #region Movement

        Vector3 currentVector = body.up;
        Vector3 targetVector = Vector3.up;

        

        RaycastHit hit;
        if (Physics.Raycast(body.position, -body.up, out hit, GroundCheckDistance, FootWalkLayerMask))
        {
            targetVector = hit.normal;
        }

        DiffAngle = Vector3.Angle(currentVector, targetVector);
        if (DiffAngle > MaxDiffBodyAngle)
        {
            Ragdoll();
            return;
        }

        float cosAngle;
        Vector3 crossResult;
        float turnAngle;

        float averageYFoot = 0f;
        float heightestYFoot = 0f;
        for (int i = 0; i < legs.Length; i++)
        {
            Leg leg = legs[i];
            //leg.Update();

            float y = leg.FootIK.position.y;

            float yDiffTODefault = (y - body.position.y) + YDistanceToFeet;

            averageYFoot += yDiffTODefault;
            if (i == 0 || y > heightestYFoot)
                heightestYFoot = y;
        }
        averageYFoot /= legs.Length;

        Vector3 velocity = body.rigidbody.velocity;

        cosAngle = Vector3.Dot(currentVector, targetVector);

        crossResult = Vector3.Cross(currentVector, targetVector);
        crossResult.Normalize();

        turnAngle = Mathf.Acos(cosAngle);
        turnAngle = Mathf.Min(turnAngle, RotateDamping);
        turnAngle = turnAngle * Mathf.Rad2Deg;

        body.rigidbody.angularVelocity += crossResult * turnAngle * RotationSpring * delta;
        #endregion

        

        Vector3 groundPosition;
        grounded = GetGround(body.position, out groundPosition, GroundCheckDistance);
        if (!grounded)
        {
            currentHeightDamping = 0f;
            currentVelocityDamping = 0f;
            velocity += Vector3.up * Gravity * delta;
        }
        else
        {
            groundPosition += Vector3.up * YDistanceToFeet;
            if (FeetYToBodyY)
                groundPosition += Vector3.up * averageYFoot;
            velocity += (groundPosition - body.position) * HeightSpring * delta;

            if (feetOnGround < 2)
            {
                //Grounded but not enough feet, cheat to disred height
                Vector3 diff = groundPosition - body.position;
                //velocity += diff * HeightSpring * delta;
            }
        }
        
        body.rigidbody.angularVelocity -= body.rigidbody.angularVelocity * RotateVelocityDamping * delta;

        velocity.y -= velocity.y * currentHeightDamping * delta;
        velocity -= velocity * currentVelocityDamping * delta;

        body.rigidbody.velocity = velocity;
    }

    private bool GetGround(Vector3 position, out Vector3 hitPoint, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, distance, FootWalkLayerMask))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = position;
        return false;
    }
}
