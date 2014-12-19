using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class EnemyTypeInfo
{
    public string poolName = "";

    public int Points
    {
        get
        {
            return points + (int)(PointPerDifficulty * Game.DifficultyLevel);
        }
    }
    public int points = 0;
    public int PointPerDifficulty = 0;
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

[System.Serializable]
public class AttributeInfo
{
    public enum Attribute
    {
        Player_Health,
        Player_Regeneration,
        Player_Weapon_Ammo_Regeneration,
        Player_Weapon_Damage,
        Base_Health,
        Base_Regeneration,
        Base_Shield1_Health,
        Base_Shield1_Regeneration,
        Base_Shield2_Health,
        Base_Shield2_Regeneration,
        Base_Damage,
        Base_Shoot_Speed,
        Base_Range,
        Base_Rocket_Speed,
        Base_Rocket_Targeting_Speed,
        Enemy_Health,
        Enemy_Damage
    }

    public string Name = string.Empty;
    public Attribute type;
    
    private int currentPoints = 0;
    public int maxPoints = 0;

    public float defaultValue = 0f;
    public float valuePerPoint = 0f;

    public int defaultCost = 0;
    public int costPerPoint = 0;

    public int CurrentPoints
    {
        get
        {
            return currentPoints;
        }
    }

    public float CurrentValue
    {
        get
        {
            return defaultValue + valuePerPoint * CurrentPoints;
        }
    }

    public int CurrentCost
    {
        get
        {
            return defaultCost + costPerPoint * CurrentPoints;
        }
    }

    public bool AllowMore
    {
        get
        {
            return currentPoints < maxPoints;
        }
    }
    public bool AllowLess
    {
        get
        {
            return currentPoints > 0;
        }
    }

    public bool AddPoint()
    {
        if (!AllowMore)
            return false;
        currentPoints++;
        return true;
    }

    public bool TakePoint()
    {
        if (!AllowLess)
            return false;
        currentPoints--;
        return true;
    }
}

public class Game : MonoBehaviour
{

    public enum EnemyType
    {
        ROBOT1
    }

    [System.Serializable]
    public struct GameInfo
    {
        public vp_FPPlayerDamageHandler playerHealthThing;
        public PlayerHealthRegeneration healthRegen;
        public PlayerAmmiRegeneration ammoRegen;
    }

    public AttributeInfo[] attributeInfo;

    public List<EnemyTypeInfo> enemyinfos = new List<EnemyTypeInfo>();
    public Timer SilenceTimer;
    public Timer SilenceCooldownTimer;
    private float WantedEnemyTimeScale = 1f;
    public float EnemyTimeScaleChangeSpeed = 5f;

    public GameInfo gameInfo;

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
        Data.Instance.Register(this);
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

        Data.Instance.PointsChanged.Send(Points);
        Data.Instance.TimePlayedChanged.Send(TimePlayed);
        Data.Instance.DeathsChanged.Send(Deaths);

        SilenceCooldownTimer.Finish();
        SilenceTimer.Finish();

        Data.Instance.SilenceAvaible.Send();
        Data.Instance.SilenceTimeChanged.Send(SilenceTimer);
        Data.Instance.SilenceCooldownChanged.Send(SilenceCooldownTimer);

        AttributeInfo info;
        foreach (var type in (AttributeInfo.Attribute[])AttributeInfo.Attribute.GetValues(typeof(AttributeInfo.Attribute)))
        {
            info = GetAttributeInfo(type);
            if (info == default(AttributeInfo))
                continue;
            UpdateAttribute(type, info);
        }

        Pause();
    }

    #region PublicInspector
    public int MaxZombies = 10;
    #endregion

    public void Update()
    {
        if (InputController.GetClicked("PAUSE"))
        {
            if (!Paused)
                Pause();
            else
                Resume();
        }

        if (InputController.GetClicked("SILENCE"))
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

    public AttributeInfo GetAttributeInfo(AttributeInfo.Attribute type)
    {
        return attributeInfo.FirstOrDefault(t => t.type == type);
    }

    public void UpdateAttribute(AttributeInfo.Attribute type, AttributeInfo info)
    {
        float before = 0f;
        switch (type)
        {
            case AttributeInfo.Attribute.Player_Health:
                before = gameInfo.playerHealthThing.MaxHealth;
                gameInfo.playerHealthThing.MaxHealth = info.CurrentValue;
                gameInfo.playerHealthThing.CurrentHealth += gameInfo.playerHealthThing.MaxHealth - before;
                break;
            case AttributeInfo.Attribute.Player_Regeneration:
                gameInfo.healthRegen.ValuePerSecond = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Player_Weapon_Ammo_Regeneration:
                gameInfo.ammoRegen.AmmoPerSecond = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Player_Weapon_Damage:
                break;
            case AttributeInfo.Attribute.Base_Health:
                before = Base.Instance.MaxHealth;
                Base.Instance.MaxHealth = info.CurrentValue;
                Base.Instance.Health += Base.Instance.MaxHealth - before;
                break;
            case AttributeInfo.Attribute.Base_Regeneration:
                Base.Instance.HealthRegeneration = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Shield1_Health:
                before = Base.Instance.Shield1.MaxHealth;
                Base.Instance.Shield1.MaxHealth = info.CurrentValue;
                Base.Instance.Shield1.Health += Base.Instance.Shield1.MaxHealth - before;
                break;
            case AttributeInfo.Attribute.Base_Shield1_Regeneration:
                Base.Instance.Shield1.HealthRegeneration = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Shield2_Health:
                before = Base.Instance.Shield2.MaxHealth;
                Base.Instance.Shield2.MaxHealth = info.CurrentValue;
                Base.Instance.Shield2.Health += Base.Instance.Shield2.MaxHealth - before;
                break;
            case AttributeInfo.Attribute.Base_Shield2_Regeneration:
                Base.Instance.Shield2.HealthRegeneration = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Damage:
                Base.Instance.RocketDamage = (int)info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Shoot_Speed:
                Base.Instance.RocketCooldown = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Range:
                Base.Instance.AutoTurretRange = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Rocket_Speed:
                Base.Instance.RocketSpeed = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Base_Rocket_Targeting_Speed:
                Base.Instance.RocketSpeed = info.CurrentValue;
                break;
            case AttributeInfo.Attribute.Enemy_Health:
                break;
            case AttributeInfo.Attribute.Enemy_Damage:
                break;
            default:
                break;
        }
    }

    void OnMessage_AttributePluss(AttributeInfo.Attribute type)
    {
        AttributeInfo info = GetAttributeInfo(type);
        if (info == null)
            return;

        if (Points < info.CurrentCost)
            return;
        int cost = info.CurrentCost;

        if (!info.AddPoint())
            return;

        Points -= cost;
        Data.Instance.PointsChanged.Send(Points);

        UpdateAttribute(type, info);
    }
    void OnMessage_AttributeMinus(AttributeInfo.Attribute type)
    {
        AttributeInfo info = GetAttributeInfo(type);
        if (info == null)
            return;
        if (!info.TakePoint())
            return;

        Points += info.CurrentCost;
        Data.Instance.PointsChanged.Send(Points);

        UpdateAttribute(type, info);
    }

    public string GetAttributeText(AttributeInfo.Attribute type)
    {
        AttributeInfo info = GetAttributeInfo(type);
        if (info == null)
            return "";

        string name = "";
        string value = "";

        name = info.Name;

        switch (type)
        {
            case AttributeInfo.Attribute.Player_Health:
                value = string.Format("{0}HP", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Player_Regeneration:
                value = string.Format("{0:0.#}/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Player_Weapon_Ammo_Regeneration:
                value = string.Format("{0:0.#}/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Player_Weapon_Damage:
                break;
            case AttributeInfo.Attribute.Base_Health:
                value = string.Format("{0}", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Regeneration:
                value = string.Format("{0:0.#}/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Shield1_Health:
                value = string.Format("{0}", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Shield1_Regeneration:
                value = string.Format("{0:0.#}/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Shield2_Health:
                value = string.Format("{0}", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Shield2_Regeneration:
                value = string.Format("{0:0.#}/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Damage:
                value = string.Format("{0}", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Shoot_Speed:
                value = string.Format("{0:0.#}s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Range:
                value = string.Format("{0:0.#}m", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Rocket_Speed:
                value = string.Format("{0}m/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Base_Rocket_Targeting_Speed:
                value = string.Format("{0}°/s", info.CurrentValue);
                break;
            case AttributeInfo.Attribute.Enemy_Health:
                break;
            case AttributeInfo.Attribute.Enemy_Damage:
                break;
            default:
                break;
        }
        return string.Format("{0}: <color=#22ff55>{1}</color>", name, value);
    }
    public string GetAttributeDescription(AttributeInfo.Attribute type)
    {
        AttributeInfo info = GetAttributeInfo(type);
        if (info == null)
            return "";

        string value = "";
        string cost = "";

        value = string.Format("{0:0.##}", info.valuePerPoint);
        cost = string.Format("{0}", info.CurrentCost);

        return string.Format("Add or Remove <color=#22AAff>{0}</color>, Takes or Gives you <color=#ff2255>{1}</color> points.", value, cost);
    }

    public AudioSource SlowDown;
    private void ActivateSilence()
    {
        SilenceCooldownTimer.Reset();
        SilenceTimer.Reset();
        WantedEnemyTimeScale = 0f;
        Data.Instance.SilenceUsed.Send();


        SlowDown.pitch = 2;
        SlowDown.Play();
    }

    private void DeactivateSilence()
    {
        SlowDown.pitch = -2;
        if (SlowDown.pitch < 0)
            SlowDown.time = SlowDown.clip.length;
        SlowDown.Play();

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

    public bool Paused = false;

    public void Pause()
    {
        Paused = true;
        Time.timeScale = 0f;
        TimeScale = 0f;
        Data.Instance.UIStateChanged.Send(GameUI.State.MENU);
    }
    public void Resume()
    {
        Time.timeScale = 1f;
        TimeScale = 1f;
        Paused = false;
        Data.Instance.UIStateChanged.Send(GameUI.State.GAME);
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
        int pointsAdd = Instance.enemyinfos[(int)type].Points;
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
