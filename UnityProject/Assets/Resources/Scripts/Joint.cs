using UnityEngine;
using System.Collections;

public class Joint : MonoBehaviour 
{
	[Range(0f, 20f)]
	public float PositionSpring = 0f;
	[Range(0f, 2f)]
	public float PositionDamping = 0f;

	[Range(0f, 10f)]
	public float RotationSpring = 0f;
	[Range(0f, 1f)]
	public float RotationDamping = 0f;

	public bool World = false;
	public bool TakeStartValues = true;
	public Vector3 WantedPosition;
	public Vector3 WantedRotation;

	// Use this for initialization
	void Start () 
	{
		if (TakeStartValues)
		{
			WantedPosition = transform.localPosition;
			WantedRotation = transform.forward;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
        FreezeLocalPosition();
	}

	void FreezeLocalPosition()
	{
		Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
		localVelocity.x = 0;
		localVelocity.z = 0;
		localVelocity.y = 0;
		rigidbody.velocity = transform.TransformDirection(localVelocity);
	}

    void LateUpdate()
    {
        FreezeLocalPosition();
    }

	void FixedUpdate()
	{
		Vector3 PositionDiff = WantedPosition - transform.localPosition;

		Vector3 force = (PositionDiff * PositionSpring) - (rigidbody.velocity * PositionDamping);
		force *= (Time.fixedDeltaTime / 0.020f);
		rigidbody.velocity = rigidbody.velocity + force;

		Debug.DrawRay(transform.position, WantedRotation);
		Debug.DrawRay(transform.position, transform.forward, Color.green);

		Vector3 RotationDiff;
		RotationDiff = Quaternion.FromToRotation(transform.forward, WantedRotation).eulerAngles;

		Vector3 axis;
		float angle;
		Quaternion.FromToRotation(transform.forward, WantedRotation).ToAngleAxis(out angle, out axis);
		RotationDiff = axis * Mathf.Clamp01(angle / 180f);

		Vector3 torgue = (RotationDiff * RotationSpring) - (rigidbody.angularVelocity * PositionDamping);
		torgue *= (Time.fixedDeltaTime / 0.020f);

		//rigidbody.AddTorque(torgue);
		rigidbody.angularVelocity = rigidbody.angularVelocity + torgue;
	}
}
