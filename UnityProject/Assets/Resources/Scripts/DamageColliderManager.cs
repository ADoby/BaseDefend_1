using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageColliderManager : MonoBehaviour
{
    public HealthHandler healthScript;

    public hitPartInfo[] partInfo;

    public float forceMultToDamage = 1.0f;
    public float DefaultMinForceForRagdoll = 5.0f;
    public float MinForceMultipliedByMass = 0.5f;
    public float DefaultDamageMult = 1.0f;

    List<Transform> colliders = new List<Transform>();

    // Use this for initialization
    void Start()
    {
        Collider[] rigidbodies = GetComponentsInChildren<Collider>();

        foreach (var child in rigidbodies)
        {
            ColliderHit script = child.gameObject.AddComponent<ColliderHit>();
            script.damageMultiplier = GetMultiplier(child.gameObject);
            script.minForceForRagdoll = GetMinForceForRagdoll(child.gameObject);
            script.healthScript = healthScript;
            script.forceMultToDamage = forceMultToDamage;

            script.owner = this;

            child.gameObject.tag = "Enemy";

            colliders.Add(child.transform);
        }
    }

    private float GetMultiplier(GameObject go)
    {
        foreach (var item in partInfo)
        {
            if (go.name.Contains(item.name))
                return item.damageMultiply;
        }
        float compute = DefaultDamageMult;
        return compute;
    }

    private float GetMinForceForRagdoll(GameObject go)
    {
        foreach (var item in partInfo)
        {
            if (go.name.Contains(item.name))
                return item.minHitForceForRagdoll;
        }
        float compute = DefaultMinForceForRagdoll;
        if (go.rigidbody)
            compute += go.rigidbody.mass * MinForceMultipliedByMass;
        return compute;
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