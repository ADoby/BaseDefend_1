using UnityEngine;
using System.Collections;

public class Enemy : HealthHandler 
{
    public Game.EnemyType MyType;

    public float HealthPerDifficulty = 50f;

    public float DefaultDamage = 10f;
    public float DamagePerDifficulty = 200f;
    protected override void Awake()
    {
        Reset();
    }

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

    public override void Reset()
    {
        CurrentMaxHealth = DefaultHealth + HealthPerDifficulty * Game.DifficultyLevel;
        base.Reset();
    }

    public void Kill()
    {
        Damage(CurrentMaxHealth + 10f);
    }
}
