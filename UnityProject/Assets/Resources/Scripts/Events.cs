using UnityEngine;
using System.Collections;

public class Events : vp_EventHandler
{

    #region Singleton
    private static Events instance;
    public static Events Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<Events>();
            return instance;
        }
    }
    protected override void Awake()
    {
        instance = this;
        base.Awake();
    }
    #endregion

    public vp_Attempt<Game.EnemyType> SpawnZombie;
    public vp_Attempt ActivateSilence;

    public vp_Message<Enemy> EnemySpawned;
    public vp_Message<Enemy> EnemyDespawned;
    public vp_Message<Enemy> EnemyDied;

    public vp_Message<vp_PlayerDamageHandler> PlayerSpawned;
    public vp_Message<vp_PlayerDamageHandler> PlayerDied;
    public vp_Message<vp_PlayerDamageHandler> PlayerDisconnected;

    public vp_Message<HealthHandler> BaseHealthChanged;
    public vp_Message<float> TimePlayedChanged;
    public vp_Message<GameUI.State> UIStateChanged;

    public vp_Message<int> PointsChanged;
    public vp_Message<int> DeathsChanged;

    public vp_Message<Timer> SilenceCooldownChanged;
    public vp_Message<Timer> SilenceTimeChanged;
    public vp_Message SilenceAvaible;
    public vp_Message SilenceUsed;
    public vp_Message SilenceEnded;

    public vp_Message<float> EnemyFixedDeltaTimeChanged;

    public vp_Message<AttribInfo.Attribute> AttributePluss;
    public vp_Message<AttribInfo.Attribute> AttributeMinus;
}
