using UnityEngine;
using System.Collections;

public class AutoRunHelper : MonoBehaviour {

    vp_FPInput input;

    protected vp_FPPlayerEventHandler m_FPPlayer = null;	// should never be referenced directly
    protected vp_FPPlayerEventHandler FPPlayer	// lazy initialization of the event handler field
    {
        get
        {
            if (m_FPPlayer == null)
                m_FPPlayer = transform.GetComponent<vp_FPPlayerEventHandler>();
            return m_FPPlayer;
        }
    }

	// Use this for initialization
	void Start ()   
    {
        input = GetComponent<vp_FPInput>();
	}

    public LayerMask walkLayer;
    public float AutoJumpMinHeight = 0.6f;
    public float AutoJumpHeight = 1.2f;
    public float AutoJumpDistance = 1.0f;

	// Update is called once per frame
	void Update ()
    {
        if (FPPlayer.InputMoveVector.Get().y > 0 && FPPlayer.Run.Active)
        {
            if (Physics.Raycast(transform.position + Vector3.up * AutoJumpMinHeight, transform.forward, AutoJumpDistance, walkLayer))
            {
                if (!Physics.Raycast(transform.position + Vector3.up * AutoJumpHeight, transform.forward, AutoJumpDistance, walkLayer))
                {
                    input.TryJumpHere();
                }
            }
        }
	}
}
