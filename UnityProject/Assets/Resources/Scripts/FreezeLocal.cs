using UnityEngine;
using System.Collections;

public class FreezeLocal : MonoBehaviour {

    public bool Position;
    public bool Rotation;

    private Vector3 position;
    private Quaternion rotation;

	// Use this for initialization
	void Start () 
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () 
    {
	}

    void FixedUpdate()
    {
    }

    void LateUpdate()
    {
        if (Position) transform.localPosition = position;
        if (Rotation) transform.localRotation = rotation;
    }
}
