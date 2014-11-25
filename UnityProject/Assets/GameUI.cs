using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour 
{
    public UIRect InGameUI;
    public UIRect InGameMenu;
    public UIRect GameOverScreen;
    public UIText pointText;
    public UIText timeText;
    public UIText deathText;

    public UIText GameOverTime;
    public UIText GameOverPoints;
    public UIText GameOverDeaths;

    public UIText MenuTime;
    public UIText MenuPoints;
    public UIText MenuDeaths;

    public enum State
    {
        MENU,
        GAME,
        GAMEOVER
    }

    private State state;

    public void BackToMenu()
    {
        Application.LoadLevel(0);
    }
    public void Restart()
    {
        Application.LoadLevel(1);
    }

	// Use this for initialization
	void Awake() {
        Data.Instance.Register(this);
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    void OnMessage_UIStateChanged(GameUI.State newState)
    {
        InGameUI.Visible = newState == State.GAME;
        InGameMenu.Visible = newState == State.MENU;
        GameOverScreen.Visible = newState == State.GAMEOVER;

        if (GameOverScreen.Visible)
        {
            GameOverTime.Text = timeText.Text;
            GameOverPoints.Text = pointText.Text;
            GameOverDeaths.Text = Game.Deaths.ToString();
        }
        if (InGameMenu.Visible)
        {
            MenuTime.Text = timeText.Text;
            MenuPoints.Text = pointText.Text;
            MenuDeaths.Text = Game.Deaths.ToString();
        }
    }

    void OnMessage_PointsChanged(int points)
    {
        pointText.Text = System.String.Format("Points: {0}", points);
    }

    void OnMessage_TimePlayedChanged(float time)
    {
        int seconds = (int)(time);
        int minutes = (int)(seconds / 60);
        int hours = (int)(minutes / 60);
        string text = System.String.Format("{0} h {1} min {2} sec", hours, minutes, seconds);
        timeText.Text = text;
    }

    void OnMessage_DeathsChanged(int deaths)
    {
        deathText.Text = System.String.Format("Deaths: {0}", deaths);
    }
}
