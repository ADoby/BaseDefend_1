using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class EnemyTypeInfo
{
    public string poolName = "";
    public int points = 0;
    public int maxSpawned = 0;
    public int currentSpawned = 0;
    public AnimationCurve SpawnChance;
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

    public List<EnemyTypeInfo> enemyinfos = new List<EnemyTypeInfo>();
    public Timer SilenceTimer;
    public Timer SilenceCooldownTimer;
    private float WantedEnemyTimeScale = 1f;
    public float EnemyTimeScaleChangeSpeed = 5f;

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
    bool OnAttempt_ActivateSilence()
    {
        return SilenceCooldownTimer.Finished;
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
        TimeScale = 1f;
        EnemyTimeScale = 1f;
        PlayerTimeScale = 1f;
        Data.Instance.UIStateChanged.Send(GameUI.State.GAME);
        Data.Instance.PointsChanged.Send(Points);
        Data.Instance.TimePlayedChanged.Send(TimePlayed);
        Data.Instance.DeathsChanged.Send(Deaths);

        SilenceCooldownTimer.Finish();
        SilenceTimer.Finish();

        Data.Instance.SilenceAvaible.Send();
        Data.Instance.SilenceTimeChanged.Send(SilenceTimer);
        Data.Instance.SilenceCooldownChanged.Send(SilenceCooldownTimer);
    }

    #region PublicInspector
    public int MaxZombies = 10;
    #endregion

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (Input.GetButtonDown("Silence"))
            TryActivatingSilence();

        if (!SilenceTimer.Finished)
        {
            SilenceTimer.Update();
            Data.Instance.SilenceTimeChanged.Send(SilenceTimer);
            if (SilenceTimer.Finished)
            {
                DeactivateSilence();
            }
        }

        if (!SilenceCooldownTimer.Finished)
        {
            SilenceCooldownTimer.Update();
            Data.Instance.SilenceCooldownChanged.Send(SilenceCooldownTimer);
            if (SilenceCooldownTimer.Finished)
            {
                Data.Instance.SilenceAvaible.Send();
            }
        }

        float newEnemyTimeScale = Mathf.Lerp(EnemyTimeScale, WantedEnemyTimeScale, Time.deltaTime * EnemyTimeScaleChangeSpeed);
        if (Mathf.Abs(newEnemyTimeScale - WantedEnemyTimeScale) < 0.05f)
            newEnemyTimeScale = WantedEnemyTimeScale;
        Data.Instance.EnemyFixedDeltaTimeChanged.Send(EnemyTimeScale - newEnemyTimeScale);
        EnemyTimeScale = newEnemyTimeScale;


        TimePlayed += Time.deltaTime;
        Data.Instance.TimePlayedChanged.Send(TimePlayed);
    }

    private void ActivateSilence()
    {
        SilenceCooldownTimer.Reset();
        SilenceTimer.Reset();
        WantedEnemyTimeScale = 0f;
        Data.Instance.SilenceUsed.Send();
    }

    private void DeactivateSilence()
    {
        WantedEnemyTimeScale = 1f;
        Data.Instance.SilenceEnded.Send();
    }

    public bool TryActivatingSilence()
    {
        if (Events.Instance.ActivateSilence.Try())
        {
            ActivateSilence();
            return true;
        }
        return false;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        TimeScale = 0f;
        Data.Instance.UIStateChanged.Send(GameUI.State.MENU);
    }
    public void Resume()
    {
        Data.Instance.UIStateChanged.Send(GameUI.State.GAME);
        Time.timeScale = 1f;
        TimeScale = 1f;
    }

    #region PublicStatic
    public static float DefaultFixedTime = 0.02f;
    public static float DefaultDeltaTime = 0.016f;

    public static int Points = 0;
    public static float TimePlayed = 0f;
    public static int Deaths = 0;

    public static float TimeScale = 1f;
    public static float PlayerTimeScale = 1f;
    public static float EnemyTimeScale = 1f;

    public static float PlayerDeltaTime
    {
        get
        {
            return (Time.deltaTime / DefaultDeltaTime) * PlayerTimeScale * TimeScale;
        }
    }
    public static float PlayerFixedDeltaTime
    {
        get
        {
            return (Time.fixedDeltaTime / DefaultFixedTime) * PlayerTimeScale * TimeScale;
        }
    }


    public static float EnemyDeltaTime
    {
        get
        {
            return Time.deltaTime * EnemyTimeScale * TimeScale;
        }
    }
    public static float EnemyDelta
    {
        get
        {
            return (Time.deltaTime / DefaultDeltaTime) * EnemyTimeScale * TimeScale;
        }
    }
    public static float EnemyFixedDeltaTime
    {
        get
        {
            return Time.fixedDeltaTime * EnemyTimeScale * TimeScale;
        }
    }
    public static float EnemyFixedDelta
    {
        get
        {
            return (Time.fixedDeltaTime / DefaultFixedTime) * EnemyTimeScale * TimeScale;
        }
    }


    public AnimationCurve DifficultyCurve;
    //3600 = 1 hour
    public float MaxDifficultyAtTime = 3600;

    //0 - 1, Difficulty
    public static float DifficultyLevel
    {
        get
        {
            return Instance.DifficultyCurve.Evaluate(Mathf.Clamp01(Mathf.Min(TimePlayed, Instance.MaxDifficultyAtTime) / Instance.MaxDifficultyAtTime));
        }
    }

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

    public static float Weight(EnemyTypeInfo info)
    {
        return info.SpawnChance.Evaluate(DifficultyLevel);
    }

    public static bool TrySpawnEnemy(Vector3 position, Quaternion rotation)
    {
        EnemyTypeInfo enemy = Instance.enemyinfos.RandomEntry(Weight);
        if (enemy == default(EnemyTypeInfo))
            return false;

        EnemyType type = (EnemyType)Instance.enemyinfos.IndexOf(enemy);

        if (Instance.enemyinfos.Count <= (int)type)
        {
            Debug.LogWarning("No Info for EnemyType: " + enemy);
            return false;
        }

        if (!enemy.AllowMore)
            return false;

        Instance.SpawnEnemy(type, position, rotation);

        return true;
    }


    public static bool TrySpawnEnemy(EnemyType enemy, Vector3 position, Quaternion rotation)
    {
        if (Instance.enemyinfos.Count <= (int)enemy)
        {
            Debug.LogWarning("No Info for EnemyType: " + enemy);
            return false;
        }

        if (!Events.Instance.SpawnZombie.Try(enemy))
            return false;

        Instance.SpawnEnemy(enemy, position, rotation);
        
        return true;
    }

    public void SpawnEnemy(EnemyType enemy, Vector3 position, Quaternion rotation)
    {
        GameObject go = GameObjectPool.Instance.Spawn(enemyinfos[(int)enemy].poolName, position, rotation);
        Enemy enemyObject = go.GetComponent<Enemy>();
        enemyObject.OnSpawn();
        EnemySpawned(enemy);
    }
}
