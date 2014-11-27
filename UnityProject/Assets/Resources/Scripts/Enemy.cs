using UnityEngine;
using System.Collections;

public class Enemy : HealthHandler 
{
    public AnimationCurve HealthCurve;

    public float DefaultDamage = 10f;
    public float MaxDifficultyDamage = 200f;
    protected virtual float CurrentDamage
    {
        get
        {
            return DefaultDamage + Game.DifficultyLevel * MaxDifficultyDamage;
        }
    }

    public virtual void OnSpawn()
    {

    }
}
