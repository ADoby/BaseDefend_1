using UnityEngine;
using System.Collections;

public class MoveTest : MonoBehaviour 
{

	public ConfigurableJoint joint;

	

	// Use this for initialization
	void Start () {

	}
	public Quaternion wantedRotation;
	// Update is called once per frame
	void Update () 
	{
		wantedRotation = Quaternion.AngleAxis(Input.GetAxis("Horizontal"), Vector3.right);

		joint.targetRotation = wantedRotation;
	}
}
