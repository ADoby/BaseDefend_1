using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageColliderManager : MonoBehaviour
{
    public Health healthScript;

    public hitPartInfo[] partInfo;

    public float forceMultToDamage = 1.0f;

    List<Transform> colliders = new List<Transform>();

    // Use this for initialization
    void Start()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody child in rigidbodies)
        {
            ColliderHit script = child.gameObject.AddComponent<ColliderHit>();
            script.damageMultiplier = GetMultiplier(child.gameObject.name);
            script.minForceForRagdoll = GetMinForceForRagdoll(child.gameObject.name);
            script.healthScript = healthScript;
            script.forceMultToDamage = forceMultToDamage;

            script.owner = this;

            child.gameObject.tag = "Enemy";

            colliders.Add(child.transform);
        }
    }

    private float GetMultiplier(string name)
    {
        foreach (var item in partInfo)
        {
            if (name.Contains(item.name))
                return item.damageMultiply;
        }
        return 1.0f;
    }

    private float GetMinForceForRagdoll(string name)
    {
        foreach (var item in partInfo)
        {
            if (name.Contains(item.name))
                return item.minHitForceForRagdoll;
        }
        return 1.0f;
    }

    public bool PartOfMe(Transform collTransform)
    {
        return colliders.Contains(collTransform);
    }
}

[System.Serializable]
public class hitPartInfo
{
    public string name;
    public float minHitForceForRagdoll;
    public float damageMultiply;
}