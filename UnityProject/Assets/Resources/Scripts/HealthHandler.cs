using UnityEngine;
using System.Collections;

public class HealthHandler : MonoBehaviour {

    public float MaxHealth;
    public float health;
    public bool alive = true;

    public float Procentage
    {
        get
        {
            return Mathf.Clamp01(health / MaxHealth);
        }
    }

    protected string poolName = "";
    public virtual void SetPoolName(string value)
    {
        poolName = value;
    }

    protected virtual void Awake()
    {
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
            alive = false;
        }
    }

    public virtual void Damage(float damage)
    {
        Damage(new vp_DamageInfo(damage, null));
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
        GameObjectPool.Instance.Despawn(poolName, gameObject);
    }

    public virtual void HitForce(float forceAmount, float minForceForRagdoll)
    {

    }
}
