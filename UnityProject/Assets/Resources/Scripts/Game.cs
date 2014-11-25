using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemyTypeInfo
{
    public string poolName = "";
    public int points = 0;
    public int maxSpawned = 0;
    public int currentSpawned = 0;
    public bool AllowMore
    {
        get
        {
            return currentSpawned < maxSpawned;
        }
    }
}

public class Game : MonoBehaviour
{

    public enum EnemyType
    {
        ROBOT1
    }

    public EnemyTypeInfo[] enemyinfos;


    #region Singleton
    private static Game instance;
    public static Game Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<Game>();
            return instance;
        }
    }
    protected void Awake()
    {
        instance = this;
    }
    #endregion

    #region Events

    protected void OnEnable()
    {
        if (Events.Instance != null)
            Events.Instance.Register(this);
    }
    protected void OnDisable()
    {
        if (Events.Instance != null)
            Events.Instance.Unregister(this);
    }
    bool OnAttempt_SpawnZombie(EnemyType type)
    {
        return enemyinfos[(int)type].AllowMore;
    }
    #endregion

    public void Start()
    {
        GameReset();
    }

    public void GameReset()
    {
        foreach (var item in enemyinfos)
        {
            item.currentSpawned = 0;
        }
        TimePlayed = 0;
        Points = 0;
        Deaths = 0;
        Time.timeScale = 1f;
        Data.Instance.UIStateChanged.Send(GameUI.State.GAME);
        Data.Instance.PointsChanged.Send(Points);
        Data.Instance.TimePlayedChanged.Send(TimePlayed);
        Data.Instance.DeathsChanged.Send(Deaths);
    }

    #region PublicInspector
    public int MaxZombies = 10;
    #endregion

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

        TimePlayed += Time.deltaTime;
        Data.Instance.TimePlayedChanged.Send(TimePlayed);
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        Data.Instance.UIStateChanged.Send(GameUI.State.MENU);
    }
    public void Resume()
    {
        Data.Instance.UIStateChanged.Send(GameUI.State.GAME);
        Time.timeScale = 1f;
    }

    #region PublicStatic
    public static float DefaultFixedTime = 0.02f;
    public static int Points = 0;
    public static float TimePlayed = 0f;
    public static int Deaths = 0;

    public static void PlayerDied()
    {
        Deaths++;
        Data.Instance.DeathsChanged.Send(Deaths);
    }
    public static void EnemySpawned(EnemyType type)
    {
        Instance.enemyinfos[(int)type].currentSpawned++;
    }
    public static void EnemyDespawned(EnemyType type)
    {
        Instance.enemyinfos[(int)type].currentSpawned--;
    }
    public static void EnemyDied(EnemyType type)
    {
        int pointsAdd = Instance.enemyinfos[(int)type].points;
        Points += pointsAdd;
        Data.Instance.OnPointsGained.Send(pointsAdd);
        Data.Instance.PointsChanged.Send(Points);
    }

    public static void TriggerGameOver()
    {
        Time.timeScale = 0f;
        Data.Instance.UIStateChanged.Send(GameUI.State.GAMEOVER);
    }
    #endregion
}
