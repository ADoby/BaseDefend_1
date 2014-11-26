using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

    #region Singleton
    private static InputHandler instance;
    public static InputHandler Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<InputHandler>();
            return instance;
        }
    }
    protected void Awake()
    {
        instance = this;
        OnAwake();
    }
    #endregion

    public vp_FPInput input;

    public bool ForceMouseLock = false;
    public bool LockStatus;

	// Use this for initialization
	void OnAwake () 
    {
        Data.Instance.Register(this);
	}
	
	// Update is called once per frame
	void Update () {
	//MouseCursorForced
        LockStatus = Screen.lockCursor;
	}

    void OnMessage_UIStateChanged(GameUI.State newState)
    {
        if(!enabled)
            return;
        if (newState == GameUI.State.GAMEOVER || newState == GameUI.State.MENU)
        {
            ForceMouseLock = true;
            Screen.lockCursor = false;
        }
        else
        {
            ForceMouseLock = false;
            Screen.lockCursor = true;
        }
    }
}
