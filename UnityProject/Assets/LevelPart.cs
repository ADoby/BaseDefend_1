using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class LevelPart : MonoBehaviour 
{
    public delegate void LevelPartEvent();
    public event LevelPartEvent OnPartStart;
    public event LevelPartEvent OnPartStop;

    public float Length = 10f;


    private bool Initiated = false;

    void Awake()
    {
        if (Init()) Despawn();
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


    public virtual void Spawn(float CurrentZPos)
    {
        Init();
        transform.position = Vector3.forward * CurrentZPos;
        gameObject.SetActive(true);
    }

    public virtual void Start()
    {
        if (OnPartStart != null) OnPartStart();
    }

    public virtual void Stop()
    {
        if (OnPartStop != null) OnPartStop();
    }

    public virtual void Despawn()
    {
        gameObject.SetActive(false);
    }
}
