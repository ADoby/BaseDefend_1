using UnityEngine;
using System.Collections;

[System.Serializable]
public class LightInfo
{
    public void Init()
    {
        defaultIntensity = light.intensity;
    }

    public Light light;
    public float defaultIntensity = 1f;

    public void SetLightValue(float value)
    {
        light.intensity = defaultIntensity * value;
    }
}

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

    public UIText SilenceTimeText;
    public UIText SilenceCooldownTimeText;

    public UIRect SilenceTime;
    public UIRect SilenceCooldown;

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
        foreach (var item in lights)
        {
            item.Init();
        }
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

    void OnMessage_SilenceCooldownChanged(Timer timer)
    {
        SilenceCooldownTimeText.Text = string.Format("Cooldown: {0:0.0}s", timer.Value - timer.CurrentTime);
        SilenceCooldown.RelativeSize.x = timer.Procentage;
        SilenceCooldown.Visible = SilenceCooldown.absoluteRect.width > 4;
    }

    void OnMessage_SilenceTimeChanged(Timer timer)
    {
        SilenceTimeText.Text = string.Format("Time left: {0:0.0}s", timer.Value - timer.CurrentTime);
        SilenceTime.RelativeSize.x = 1f - timer.Procentage;
        SilenceTime.Visible = SilenceTime.absoluteRect.width > 4;
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

		seconds -= minutes * 60;
		minutes -= hours * 60;

		string text = System.String.Format("{0} h {1} min {2} sec", hours, minutes, seconds);
		timeText.Text = text;
	}

	void OnMessage_DeathsChanged(int deaths)
	{
		deathText.Text = System.String.Format("Deaths: {0}", deaths);
	}

    public LightInfo[] lights;
    public Camera playerCam;
    public Color DarkColor, LightColor;


    public BaseShield[] shields;


    public UIText BaseRegenerationText;
    public UIText BaseShieldRegenerationText;

    public void Message_LightSliderChanged(UIFloatSlider slider)
    {
        if (slider.name.Equals("Light"))
        {
            playerCam.backgroundColor = Color.Lerp(DarkColor, LightColor, slider.Procentage);
            foreach (var item in lights)
            {
                item.SetLightValue(slider.currentValue);
            }
        }
        else if (slider.name.Equals("BaseRegeneration"))
        {
            BaseRegenerationText.Text = string.Format("Base Regeneration: {0:0.0}/s", slider.currentValue);
            Base.Instance.HealthRegeneration = slider.currentValue;
        }
        else if (slider.name.Equals("BaseShieldRegeneration"))
        {
            BaseShieldRegenerationText.Text = string.Format("Base Shield Regeneration: {0:0.0}/s", slider.currentValue);
            foreach (var item in shields)
            {
                item.DefaultShieldRegeneration = slider.currentValue;
            }
        }
    }
}
