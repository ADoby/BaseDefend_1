using UnityEngine;
using System.Collections;

[System.Serializable]
public class Leg
{
    private const float PI = 3.1415f;
    public string Name;

    public RobotBody Owner;
    public Transform Root;
    public Transform Foot;
    public Transform FootIK;
    public float MaxDistance = 0.2f;
    public float TransitionTime = 1.0f;
    public float TransitionYDiff = 0.5f;
    public float TransitionDamping = 1.0f;
    public float WaitTime = 0.5f;

    public Vector4 moveRestriction;

    //DEBUG
    public float CurrentDistance = 0f;

    private float TransitionTimer;
    private float WaitTimer;
    private Vector3 LastPosition;
    private Vector3 WantedPosition;

    private float lastY = 0f;

    public void Start()
    {
        WantedPosition = FootIK.position;
        LastPosition = WantedPosition;
        TransitionTimer = TransitionTime;
        WaitTimer = WaitTime;
        lastY = WantedPosition.y;
    }

    private bool NeedsNewPosition()
    {
        Vector3 diff = FootIK.position - Foot.position; 
        diff.y = 0;
        CurrentDistance = diff.magnitude;
        return (CurrentDistance > MaxDistance);
    }

    private bool GetGround(Vector3 position, out Vector3 hitPoint)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 2f, Owner.FootWalkLayerMask))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = position;
        return false;
    }

    public void Update()
    {
        GetGround(WantedPosition, out WantedPosition);

        TransitionTimer = Mathf.Min(TransitionTimer + Time.deltaTime, TransitionTime);
        float procentage = (TransitionTimer / TransitionTime);
        float sinVal = Mathf.Sin(procentage * PI);

        Vector3 wantedPos = procentage < 0.5f ? LastPosition : WantedPosition;
        wantedPos.y += sinVal * TransitionYDiff;

        Vector3 diff = wantedPos - FootIK.position;
        diff = diff.normalized * Mathf.Min(TransitionDamping, diff.magnitude);
        FootIK.position += diff;

        if (procentage >= 1f)
            WaitTimer = Mathf.Min(WaitTimer + Time.deltaTime, WaitTime);

        if (WaitTimer >= WaitTime && NeedsNewPosition())
        {
            Vector3 direction = Foot.position - FootIK.position;
            direction = Owner.body.rigidbody.velocity.magnitude > 0f ? Owner.body.rigidbody.velocity.normalized : direction;
            direction.y = 0;
            direction.Normalize();

            Vector3 newPos = WantedPosition + direction * 1000f;
            Vector3 min = Owner.body.rotation * new Vector3(moveRestriction.x, 0, moveRestriction.z);
            Vector3 max = Owner.body.rotation * new Vector3(moveRestriction.y, 0, moveRestriction.w);
            newPos.x = Mathf.Clamp(newPos.x, Root.position.x + min.x, Root.position.x + max.x);
            newPos.z = Mathf.Clamp(newPos.z, Root.position.z + min.z, Root.position.z + max.z);

            if (GetGround(newPos, out newPos))
            {
                LastPosition = WantedPosition;
                WantedPosition = newPos;
                TransitionTimer = 0f;
                WaitTimer = 0f;
            }
        }
    }
}

public class RobotBody : MonoBehaviour 
{
    public LayerMask FootWalkLayerMask;
    public Leg[] legs = {};

	// Use this for initialization
	void Start () 
    {
        foreach (var item in legs)
        {
            item.Owner = this;
            item.Start();
        }
	}

    public float MoveSpeed = 2.0f;
    public float RotateSpeed = 2.0f;

    public Transform body;

    [Range(0f, 1f)]
    public float VelocityDamping = 0.2f;
    [Range(0f, 1.0f)]
    public float RotateVelocityDamping = 0.2f;

    public float GroundDistance = 0.75f;
    [Range(0f, 2f)]
    public float HeightSpring = 5.0f;
    public float HeightDamping = 1.0f;

    [Range(0f, 0.25f)]
    public float RotationSpring = 3.0f;
    public float RotateDamping = 1.0f;
    
    public float YDistanceToFeet = 1.0f;
    public float FeetSpring = 5.0f;
    public float FeetDamping = 1.0f;

	// Update is called once per frame
	void Update ()
    {
        float heightestYFoot = 0f;
        for (int i = 0; i < legs.Length; i++)
        {
            Leg leg = legs[i];
            leg.Update();

            float y = leg.Foot.position.y;

            if (i == 0 || y > heightestYFoot)
                heightestYFoot = y;
            
        }
	}

    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime / 0.02f;

        Vector3 velocity = body.rigidbody.velocity;

        Vector3 moveDirection = Input.GetAxis("Vertical") * body.forward;
        velocity += moveDirection * MoveSpeed * delta;
        Vector3 rotation = Input.GetAxis("Horizontal") * Vector3.up;
        body.rigidbody.angularVelocity += rotation * RotateSpeed * delta;

        Vector3 currentVector = body.up;
        Vector3 targetVector = body.up;
        targetVector.x = 0;
        targetVector.z = 0;

        float cosAngle;
        Vector3 crossResult;
        float turnAngle;

        float heightestYFoot = 0f;
        for (int i = 0; i < legs.Length; i++)
        {
            Leg leg = legs[i];
            //leg.Update();

            float y = leg.Foot.position.y;

            if (i == 0 || y > heightestYFoot)
                heightestYFoot = y;

            //float upForce = (body.position.y + YDistanceToFeet) - y;
            //upForce = Mathf.Min(FeetDamping, upForce);

            Vector3 targetV = leg.Foot.position - body.position;
            //targetV = Quaternion.LookRotation(-targetV).eulerAngles;
            //targetV.y = 0;

            cosAngle = Vector3.Dot(currentVector, targetV);

            crossResult = Vector3.Cross(currentVector, targetV);
            crossResult.Normalize();

            turnAngle = Mathf.Acos(cosAngle);
            turnAngle = Mathf.Min(turnAngle, FeetDamping);
            turnAngle = turnAngle * Mathf.Rad2Deg;

            //crossResult = Vector3.Reflect(currentVector, targetV);

            //targetVector += crossResult * turnAngle * FeetSpring * delta;

            //Vector3 cross = Vector3.Cross(body.up, targetV);
            //cross.Normalize();
            //body.rigidbody.AddForceAtPosition(Vector3.up * upForce * FeetSpring, leg.Root.position);
        }


        cosAngle = Vector3.Dot(currentVector, targetVector);

        crossResult = Vector3.Cross(currentVector, targetVector);
        crossResult.Normalize();

        turnAngle = Mathf.Acos(cosAngle);
        turnAngle = Mathf.Min(turnAngle, RotateDamping);
        turnAngle = turnAngle * Mathf.Rad2Deg;

        body.rigidbody.angularVelocity += crossResult * turnAngle * RotationSpring * delta;


        Vector3 groundPos;
        if (GetGround(body.position, out groundPos))
            groundPos += Vector3.up * GroundDistance;
        else
            groundPos -= Vector3.up;

        velocity += (groundPos - body.position) * HeightSpring * delta;

        Vector3 wantedPosition = body.position;
        wantedPosition.y = heightestYFoot + YDistanceToFeet;
        Vector3 difference = (wantedPosition - body.position);
        float dampedSpeed = Mathf.Min(difference.magnitude, HeightDamping);
        //velocity += difference.normalized * HeightSpring * delta * dampedSpeed;

        body.rigidbody.angularVelocity -= body.rigidbody.angularVelocity * RotateVelocityDamping * delta;
        velocity -= velocity * VelocityDamping * delta;
        body.rigidbody.velocity = velocity;
    }

    private bool GetGround(Vector3 position, out Vector3 hitPoint)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 10f, FootWalkLayerMask))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = position;
        return false;
    }
}
