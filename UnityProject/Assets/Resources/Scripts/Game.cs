using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{

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
        GameReset();
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
    bool OnAttempt_SpawnZombie()
    {
        Debug.Log("Spawn?=: " + NewSpawnAllowed);
        return NewSpawnAllowed;
    }
    #endregion

    public void GameReset()
    {
        MaxZombieCount = MaxZombies;
        currentZombieCount = 0;
    }

    #region PublicInspector
    public int MaxZombies = 10;
    #endregion

    #region PublicStatic
    public static float DefaultFixedTime = 0.02f;

    public static bool NewSpawnAllowed
    {
        get
        {
            return currentZombieCount < MaxZombieCount;
        }
    }
    public static void ZombieSpawned(GameObject zombie)
    {
        currentZombieCount++;
    }
    public static void ZombieDespawned(GameObject zombie)
    {
        currentZombieCount--;
    }
    #endregion

    #region SpawnInfos
    public static int MaxZombieCount = 10;
    #endregion

    private static int currentZombieCount = 0;
}
