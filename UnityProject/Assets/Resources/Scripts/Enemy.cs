using UnityEngine;
using System.Collections;

public class Enemy : HealthHandler 
{
    public AnimationCurve HealthCurve;

    public float DefaultDamage = 10f;
    public float DamagePerDifficulty = 200f;
    public float CurrentDamage
    {
        get
        {
            return DefaultDamage + DamagePerDifficulty * Game.DifficultyLevel;
        }
    }

    public virtual void OnSpawn()
    {

    }
}
