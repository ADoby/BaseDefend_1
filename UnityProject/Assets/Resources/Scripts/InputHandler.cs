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

    public MyInput input;

    public bool ForceMouseLock = true;
    public bool WantedLockStatus = false;

	// Use this for initialization
	void OnAwake () 
    {
        Data.Instance.Register(this);
	}
	

    void OnMessage_UIStateChanged(GameUI.State newState)
    {
        if(!enabled)
            return;
        
        if (newState == GameUI.State.GAMEOVER || newState == GameUI.State.MENU || newState == GameUI.State.REBINDKEY || newState == GameUI.State.SHOPPLAYER || newState == GameUI.State.SHOPBASE || newState == GameUI.State.SHOPENEMIES)
        {
            WantedLockStatus = false;
        }
        else
        {
            WantedLockStatus = true;
        }
    }
}
