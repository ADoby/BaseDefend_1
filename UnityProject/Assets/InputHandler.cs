using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

    public vp_FPInput input;

	// Use this for initialization
	void Awake () {
        Data.Instance.Register(this);
	}
	
	// Update is called once per frame
	void Update () {
	//MouseCursorForced
	}

    void OnMessage_UIStateChanged(GameUI.State newState)
    {
        if (newState == GameUI.State.GAMEOVER || newState == GameUI.State.MENU)
        {
            input.MouseCursorForced = true;
            Screen.lockCursor = false;
        }
        else
        {
            input.MouseCursorForced = false;
            Screen.lockCursor = true;
        }
    }
}
