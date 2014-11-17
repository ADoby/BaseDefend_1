using UnityEngine;
using System.Collections;

public class UpdateAnchorPosition : MonoBehaviour 
{

	public ConfigurableJoint joint;

	private Vector3 startPosition;

	// Use this for initialization
	void Start () {
		startPosition = transform.InverseTransformPoint(joint.connectedAnchor);
	}
	
	// Update is called once per frame
	void Update () 
	{
		joint.connectedAnchor = transform.TransformPoint(startPosition);
	}

	void FixedUpdate()
	{   
		joint.connectedAnchor = transform.TransformPoint(startPosition);
	}

	void LateUpdate()
	{
		joint.connectedAnchor = transform.TransformPoint(startPosition);
	}
}
