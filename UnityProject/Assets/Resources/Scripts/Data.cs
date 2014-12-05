using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data : vp_EventHandler
{
	#region Singleton
	private static Data instance;
	public static Data Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<Data>();
			return instance;
		}
	}
	protected void Awake()
	{
		instance = this;
		base.Awake();
	}
	#endregion

	public vp_Message<HealthHandler> BaseHealthChanged;
    public vp_Message<int> PointsChanged;
    public vp_Message<float> TimePlayedChanged;
    public vp_Message<GameUI.State> UIStateChanged;
    public vp_Message<int> DeathsChanged;

    public vp_Message<Timer> SilenceCooldownChanged;
    public vp_Message<Timer> SilenceTimeChanged;
    public vp_Message SilenceAvaible;
    public vp_Message SilenceUsed;
    public vp_Message SilenceEnded;

    public vp_Message<float> EnemyFixedDeltaTimeChanged;

    public vp_Message<int> OnPointsGained;

    public vp_Message<AttributeInfo.Attribute> AttributePluss;
    public vp_Message<AttributeInfo.Attribute> AttributeMinus;

}
