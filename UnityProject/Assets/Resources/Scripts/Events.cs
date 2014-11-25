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

}
