using UnityEngine;
using System.Collections;

public class HoldPositionRigidbody : MonoBehaviour 
{

    public bool Ragdolled = false;

    public bool RotateTowards = true;
    public float LookSpring = 2f;
    public float MaxLookForce = 0.5f;


    public float Spring_Height = 100f;
    public float MaxForcePerUpdate_Height = 40f;
    public float Damping_Height = 0.8f;

    public float Spring_Plane = 0.5f;
    public float MaxForcePerUpdate_Plane = 2.0f;
    public float Damping_Plane = 0.5f;

    public float RotationSpring = 0.5f;
    public float MaxRotationPerUpdate = 2.0f;
    public float RotationDamping = 0.5f;

    public Transform targetPosition;

    public float AngleDiff = 0f;
    public float Distance = 0f;

    void FixedUpdate()
    {
        float delta = Game.EnemyFixedDelta;

        if (Ragdolled)
            return;

        Vector3 currentVelocity = rigidbody.velocity;

        Vector3 targetVector = Vector3.zero;
        Vector3 currentVector = Vector3.zero;
        Vector3 crossResult = Vector3.zero;
        float cosAngle = 0f, turnAngle = 0f;


        targetVector = Vector3.up;
        currentVector = transform.up;
        cosAngle = Vector3.Dot(currentVector, targetVector);
        crossResult = Vector3.Cross(currentVector, targetVector);
        crossResult.Normalize();
        turnAngle = Mathf.Acos(cosAngle);
        turnAngle = Mathf.Min(turnAngle * RotationSpring * delta, MaxRotationPerUpdate);
        turnAngle = turnAngle * Mathf.Rad2Deg;
        rigidbody.angularVelocity += crossResult * turnAngle;

        AngleDiff = (cosAngle) * 180f;

        if (RotateTowards)
        {
            targetVector = (targetPosition.position - transform.position).normalized;
            currentVector = transform.forward;
            cosAngle = Vector3.Dot(currentVector, targetVector);
            crossResult = Vector3.Cross(currentVector, targetVector);
            crossResult.Normalize();
            turnAngle = Mathf.Acos(cosAngle);
            turnAngle = Mathf.Min(turnAngle * LookSpring * delta, MaxLookForce);
            turnAngle = turnAngle * Mathf.Rad2Deg;
            crossResult.x = 0f;
            crossResult.z = 0f;
            rigidbody.angularVelocity += crossResult * turnAngle;
        }

        Vector3 direction = targetPosition.position - transform.position;
        Distance = Mathf.Abs(direction.magnitude);
        float heightDiff = direction.y;
        direction.y = 0f;
        direction.Normalize();

        if (RotateTowards)
        {
            direction *= ((180f - Mathf.Abs(turnAngle)) / 180f);
        }

        currentVelocity += direction * Mathf.Min(Distance * Spring_Plane * delta, MaxForcePerUpdate_Plane);


        currentVelocity += heightDiff * Vector3.up * Mathf.Min(Distance * Spring_Height * delta, MaxForcePerUpdate_Height);




        rigidbody.angularVelocity -= rigidbody.angularVelocity * RotationDamping * delta;

        currentVelocity.y -= currentVelocity.y * Damping_Height * delta;
        currentVelocity -= new Vector3(currentVelocity.x, 0, currentVelocity.z) * Damping_Plane * delta;
        rigidbody.velocity = currentVelocity;
    }
}
