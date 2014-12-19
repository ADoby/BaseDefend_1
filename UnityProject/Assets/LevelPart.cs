using UnityEngine;
using System.Collections;

public class LevelPart : MonoBehaviour 
{
    public float Length = 10f;

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
