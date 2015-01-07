using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class LevelPart : MonoBehaviour 
{
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
        if (CheckWonTimer.Update())
        {
            CheckWonTimer.Reset();
            if (Won())
            {
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
    }

    public virtual void StopPart()
    {
        IsActive = false;
        if(Exit) Exit.Open();
        if (OnPartStop != null) OnPartStop();
    }

    public virtual void DespawnPart()
    {
        IsActive = false;
        gameObject.SetActive(false);
    }
}
