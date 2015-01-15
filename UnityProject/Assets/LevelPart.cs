using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class LevelPart : MonoBehaviour 
{
    public Pathfinder PathFinder;

    public delegate void LevelPartEvent();
    public event LevelPartEvent OnPartStart;
    public event LevelPartEvent OnPartStop;
    public bool IsActive = false;

    public Door Entrance;
    public Door Exit;

    public float Length = 10f;

    [SerializeField]
    public List<WinCondition> WinConditions = new List<WinCondition>();

    public bool Won()
    {
        foreach (var condition in WinConditions)
        {
            if (!condition.Finished)
                return false;
            else
                Game.Instance.RemoveCondition(condition);
        }
        return true;
    }

    private bool Initiated = false;

    void Awake()
    {
        if (Init()) DespawnPart();
    }

    public bool Init()
    {
        if (Initiated)
            return false;
        Renderer[] childRenders = GetComponentsInChildren<Renderer>().Where(t => t.tag == "static").ToArray();
        List<GameObject> childGos = new List<GameObject>();
        foreach (var item in childRenders)
        {
            childGos.Add(item.gameObject);
        }
        StaticBatchingUtility.Combine(childGos.ToArray(), gameObject);
        Initiated = true;
        return true;
    }

    public Timer CheckWonTimer;

    void Update()
    {
        if (!IsActive)
            return;

        if (CheckWonTimer.Update())
        {
            CheckWonTimer.Reset();
            if (Won())
            {
                Game.Instance.PartFinished();
                StopPart();
            }
        }
    }

    public virtual void SpawnPart(float CurrentZPos)
    {
        Init();
        transform.position = Vector3.forward * CurrentZPos;
        gameObject.SetActive(true);

        if (Entrance) Entrance.Open();
        if (Exit) Exit.Close();
    }

    public virtual void StartPart()
    {
        IsActive = true;
        if(Entrance) Entrance.Close();
        if (OnPartStart != null) OnPartStart();

        foreach (var item in WinConditions)
        {
            item.Start();
            Game.Instance.AddCondition(item);
        }
        Events.Instance.Register(this);
    }

    public virtual void StopPart()
    {
        IsActive = false;
        if(Exit) Exit.Open();
        if (OnPartStop != null) OnPartStop();

        foreach (var item in WinConditions)
        {
            item.Stop();
            Game.Instance.RemoveCondition(item);
        }
        Events.Instance.Unregister(this);

        foreach (var enemy in SpawnedEnemies)
        {
            enemy.Despawn();
        }
        SpawnedEnemies.Clear();
    }

    public virtual void DespawnPart()
    {
        IsActive = false;
        gameObject.SetActive(false);
        Events.Instance.Unregister(this);
    }

    public List<Enemy> SpawnedEnemies = new List<Enemy>();

    public void OnMessage_EnemySpawned(Enemy enemy)
    {
        if (!SpawnedEnemies.Contains(enemy))
            SpawnedEnemies.Add(enemy);
    }
    public void OnMessage_EnemyDespawned(Enemy enemy)
    {
        if (SpawnedEnemies.Contains(enemy))
            SpawnedEnemies.Remove(enemy);
    }
}
