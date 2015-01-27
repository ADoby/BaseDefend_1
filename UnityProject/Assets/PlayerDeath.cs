using UnityEngine;
using System.Collections;

public class PlayerDeath : vp_Respawner 
{
    private vp_PlayerEventHandler m_Player = null;	// should never be referenced directly
    protected vp_PlayerEventHandler Player	// lazy initialization of the event handler field
    {
        get
        {
            if (m_Player == null)
                m_Player = transform.GetComponent<vp_PlayerEventHandler>();
            return m_Player;
        }
    }

    protected override void Die()
    {
        Reset();
    }

    protected void OnEnable()
    {
        if (Player != null)
            Player.Register(this);
    }
    protected void OnDisable()
    {
        if (Player != null)
            Player.Unregister(this);
    }

    public override void Reset()
    {

        if (!Application.isPlaying)
            return;

        if (Player == null)
            return;

        Screen.lockCursor = false;
        Application.LoadLevel(0);

    }
}
