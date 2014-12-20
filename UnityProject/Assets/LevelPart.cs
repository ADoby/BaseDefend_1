using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class LevelPart : MonoBehaviour 
{
    public float Length = 10f;

    void Awake()
    {
        Renderer[] childRenders = GetComponentsInChildren<Renderer>().Where(t => t.tag == "static").ToArray();
        
        foreach (var item in childRenders)
        {

        }
        StaticBatchingUtility.Combine(gameObject);
        Despawn();
    }

    public virtual void Spawn(float CurrentZPos)
    {
        transform.position = Vector3.forward * CurrentZPos;
        gameObject.SetActive(true);
    }

    public virtual void Start()
    {

    }

    public virtual void Reset()
    {

    }

    public virtual void Despawn()
    {
        gameObject.SetActive(false);
    }
}
