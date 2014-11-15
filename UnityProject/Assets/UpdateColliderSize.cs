using UnityEngine;
using System.Collections;

public class UpdateColliderSize : MonoBehaviour {

    public vp_FPController controller;

	// Use this for initialization
	void Awake () 
    {
        controller.targetCollider = GetComponent<CapsuleCollider>();
	}
}
