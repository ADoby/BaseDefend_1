using UnityEngine;
using System.Collections;

public class Terminal : MonoBehaviour 
{

    public bool Activated = true;
    public LayerMask playerMask;

    public float activationRadius = 2.0f;

    public bool PlayerNeedsToBeNear = true;
    public bool Shown = false;
    public UIRect overlay;

    public UIText overlayText;

	// Use this for initialization
	void Awake () 
    {
        Data.Instance.Register(this);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (GameUI.Instance.Shop.Visible)
        {
            if (InputController.GetClicked("INTERACT"))
            {
                Game.Instance.Resume();
                return;
            }
        }

        if(PlayerNeedsToBeNear)
            overlay.Visible = Activated && Shown;
        if (!Activated)
            return;
        if (PlayerNeedsToBeNear)
            CheckForPlayer();
        else
            Shown = true;
        if (Shown)
        {
            if (InputController.GetClicked("INTERACT"))
            {
                GameUI.Instance.OpenShop();
            }
        }
	}

    public void Activate()
    {
        Shown = true;
    }

    public void Deactivate()
    {
        Shown = false;
    }

    public void CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, activationRadius, playerMask);
        if (colliders.Length > 0)
            Activate();
        else
            Deactivate();
    }

    void OnMessage_UIStateChanged(GameUI.State newState)
    {
        Activated = !(newState == GameUI.State.SHOPBASE || newState == GameUI.State.SHOPENEMIES || newState == GameUI.State.SHOPPLAYER);
        if (Activated)
        {
            overlayText.Text = string.Format("Press <color=#ff2255>{0}</color> to open terminal", InputController.Instance.GetInfo("INTERACT").GetInfo());
        }
    }
}
