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

[System.Serializable]
public class RebindInfo
{
    public UIButton button;
    public string Desc = "";
    public string Action = "";
}

public class GameUI : MonoBehaviour 
{

    #region Singleton
    private static GameUI instance;
    public static GameUI Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameUI>();
            return instance;
        }
    }
    #endregion

	public UIRect InGameUI;
	public UIRect InGameMenu;
    public UIRect RebindMenu;
	public UIRect GameOverScreen;
    public UIRect Shop;
    public UIRect Shop_Player, Shop_Base, Shop_Enemies;
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

    public UIText AvaiblePoints;

	public enum State
	{
		MENU,
		GAME,
        REBINDKEY,
		GAMEOVER,
        SHOPPLAYER,
        SHOPBASE,
        SHOPENEMIES
	}

	private State state;

    private State lastShopState = State.SHOPPLAYER;

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
        instance = this;

		Events.Instance.Register(this);
        foreach (var item in lights)
        {
            item.Init();
        }

        foreach (var item in RebindInfos)
        {
            item.button.Text = string.Format("{0}: {1}", item.Desc, InputController.Instance.GetInfo(item.Action).GetInfo());
        }
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (state == State.REBINDKEY)
        {
            if(InputController.Instance.CheckForInput())
            {
                currentRebindButton.button.Text = string.Format("{0}: {1}", currentRebindButton.Desc, InputController.Instance.GetInfo(currentRebindButton.Action).GetInfo());
                currentRebindButton = null;
                Events.Instance.UIStateChanged.Send(State.MENU);
            }
        }
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
		InGameMenu.Visible = newState == State.MENU || newState == State.REBINDKEY;
		GameOverScreen.Visible = newState == State.GAMEOVER;

        RebindMenu.Visible = newState == State.REBINDKEY;
        InGameMenu.Enabled = newState == State.MENU;

        Shop.Visible = newState == State.SHOPBASE || newState == State.SHOPENEMIES || newState == State.SHOPPLAYER;

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
        if (Shop.Visible)
        {
            AvaiblePoints.Text = string.Format("Available Points:\n<color=#ff2255>{0}</color>", Game.Points);

            Shop_Player.Visible = newState == State.SHOPPLAYER;
            Shop_Base.Visible = newState == State.SHOPBASE;
            Shop_Enemies.Visible = newState == State.SHOPENEMIES;

            Tab_Player.forceHover = newState == State.SHOPPLAYER;
            Tab_Base.forceHover = newState == State.SHOPBASE;
            Tab_Enemies.forceHover = newState == State.SHOPENEMIES;

            lastShopState = newState;
        }
        state = newState;
	}

    public UIButton Tab_Player;
    public UIButton Tab_Base;
    public UIButton Tab_Enemies;

    public void OpenShop()
    {
        Events.Instance.UIStateChanged.Send(lastShopState);
    }

    void ChangeShopTab(UIButton button)
    {
        if (button == Tab_Player)
        {
            Events.Instance.UIStateChanged.Send(State.SHOPPLAYER);
        }
        else if(button == Tab_Base)
        {
            Events.Instance.UIStateChanged.Send(State.SHOPBASE);
        }
        else if (button == Tab_Enemies)
        {
            Events.Instance.UIStateChanged.Send(State.SHOPENEMIES);
        }
    }

	void OnMessage_PointsChanged(int points)
	{
		pointText.Text = System.String.Format("Points: {0}", points);
        AvaiblePoints.Text = string.Format("Available Points:\n<color=#ff2255>{0}</color>", Game.Points);
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
    }

    public RebindInfo[] RebindInfos;
    private RebindInfo currentRebindButton = null;

    public void StartRebindKey(UIButton button)
    {
        currentRebindButton = null;
        for (int index = 0; index < RebindInfos.Length; index++)
        {
            if (RebindInfos[index].button == button)
            {
                currentRebindButton = RebindInfos[index];
                break;
            }
        }
        if (currentRebindButton == null)
            return;

        InputController.Instance.RebindKey(currentRebindButton.Action);
        Events.Instance.UIStateChanged.Send(State.REBINDKEY);
    }
}
