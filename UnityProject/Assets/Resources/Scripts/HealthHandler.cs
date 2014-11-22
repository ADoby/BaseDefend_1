using UnityEngine;
using System.Collections;

public class HealthHandler : MonoBehaviour {

    public float MaxHealth;
    public float health;
    public bool alive = true;

    protected string poolName = "";
    public virtual void SetPoolName(string value)
    {
        poolName = value;
    }

    public virtual void Damage(vp_DamageInfo info)
    {
        Damage(info.Damage);
    }

    public virtual void Damage(float damage)
    {
        if (isDead)
            return;
        health = Mathf.Max(health - damage, 0);
        if (health == 0)
        {
            Die();
            alive = false;
        }
    }

    public virtual void Reset()
    {
        health = MaxHealth;
        alive = true;
    }

    public bool isDead
    {
        get
        {
            return !alive;
        }
    }
    public float Health
    {
        get
        {
            return health;
        }
    }

    public virtual void Die()
    {
        Despawn();
    }

    public virtual void Despawn()
    {
        Game.ZombieDespawned(gameObject);
        GameObjectPool.Instance.Despawn(poolName, gameObject);
    }

    public virtual void HitForce(float forceAmount, float minForceForRagdoll)
    {

    }
}
