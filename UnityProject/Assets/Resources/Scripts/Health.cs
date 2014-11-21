using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public float health;

    public virtual void Damage(vp_DamageInfo info)
    {
        health -= info.Damage;
        if (health < 0)
            health = 0;
    }

    public bool isDead()
    {
        return health <= 0;
    }

    public virtual void HitForce(float forceAmount, float minForceForRagdoll)
    {

    }
}
