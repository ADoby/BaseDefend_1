using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region DROPS
[System.Serializable]
public class WeightedItemDrop
{
    public string ItemPool = "";
    public float Weight = 0;
}

[System.Serializable]
public class WeightedDropAmount
{
    public int DropAmount = 0;
    public float Weight = 0;
}

[System.Serializable]
public class ItemDropSettings
{
    public List<WeightedItemDrop> Drops = new List<WeightedItemDrop>();
    public List<WeightedDropAmount> DropAmounts = new List<WeightedDropAmount>();

    public Vector3 MinRandomForce = Vector3.zero;
    public Vector3 MaxRandomForce = Vector3.one;
    public float RandomTourqeAmount = 10f;

    public bool DropsActivated
    {
        get
        {
            return Drops.Count > 0 && DropAmounts.Count > 0;
        }
    }

    public float DropWeight(WeightedItemDrop o)
    {
        return o.Weight;
    }
    public float DropAmountWeight(WeightedDropAmount o)
    {
        return o.Weight;
    }
    public int GetRandomDropAmount()
    {
        return DropAmounts.RandomEntry(DropAmountWeight).DropAmount;
    }
    public string GetRandomDropPool()
    {
        return Drops.RandomEntry(DropWeight).ItemPool;
    }

    public void Drop(Vector3 position)
    {
        if (!DropsActivated)
            return;

        int DropAmount = GetRandomDropAmount();
        for (int i = 0; i < DropAmount; i++)
        {
            DropItem(position, GetRandomDropPool());
        }
    }
    public void DropItem(Vector3 position, string pool)
    {
        GameObject go = GameObjectPool.Instance.Spawn(pool, position, Quaternion.identity);
        if (go)
        {
            if (go.rigidbody)
            {
                Vector3 force = Vector3.zero;
                force.x = Random.Range(MinRandomForce.x, MaxRandomForce.x);
                force.y = Random.Range(MinRandomForce.y, MaxRandomForce.y);
                force.z = Random.Range(MinRandomForce.z, MaxRandomForce.z);
                go.rigidbody.AddForce(force * go.rigidbody.mass);
                go.rigidbody.AddTorque(Random.insideUnitSphere * go.rigidbody.mass * RandomTourqeAmount);
            }
        }
    }
}

#endregion

public class HealthHandler : MonoBehaviour 
{
    public ItemDropSettings DropSettings;

    public float DefaultHealth = 0f;
    protected float CurrentMaxHealth = 0f;
    public float health;
    public bool alive = true;

    public Transform bodyThing;

    public Transform GetBody
    {
        get
        {
            return bodyThing ? bodyThing : transform;
        }
    }

    public float Procentage
    {
        get
        {
            return Mathf.Clamp01(health / CurrentMaxHealth);
        }
    }

    protected string poolName = "";
    public virtual void SetPoolName(string value)
    {
        poolName = value;
    }

    protected virtual void Awake()
    {
        CurrentMaxHealth = DefaultHealth;
        Reset();
    }

    public virtual void Damage(vp_DamageInfo info)
    {
        if (isDead)
            return;
        health = Mathf.Max(health - info.Damage, 0);
        if (health == 0)
        {
            Die();
        }
    }

    public virtual void Damage(float damage)
    {
        Damage(new vp_DamageInfo(damage, null));
    }

    public virtual void Reset()
    {
        health = CurrentMaxHealth;
        alive = true;
    }

    public bool isDead
    {
        get
        {
            return !alive;
        }
    }
    public virtual float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = Mathf.Clamp(value, 0, CurrentMaxHealth);
        }
    }

    public virtual void Die()
    {
        alive = false;
        Drop();
        Despawn();
    }

    public virtual void Drop()
    {
        DropSettings.Drop(transform.position);
    }

    public virtual void Despawn()
    {
        GameObjectPool.Instance.Despawn(poolName, gameObject);
    }

    public virtual void HitForce(float forceAmount, float minForceForRagdoll)
    {

    }
}
