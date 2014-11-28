using UnityEngine;
using System.Collections;

public class BaseShield : HealthHandler
{
    public float ChangeSpeed = 2.0f;
    private float lastHeight = 1f;
    public float WantedHeight = 1f;
    private Vector3 wantedScale;

    public float DefaultShieldRegeneration = 1.0f;
    public float ShieldRegenerationFromDifficulty = 30f;
    private float ShieldRegeneration
    {
        get
        {
            return DefaultShieldRegeneration + ShieldRegenerationFromDifficulty * Game.DifficultyLevel;
        }
    }


    protected override void Awake()
    {
        base.Awake();

        wantedScale = transform.localScale;
    }

    public override void Reset()
    {
        base.Reset();
    }

    public override void Despawn()
    {
        //Dont despawn
    }

    public void UpdateHeight()
    {
        wantedScale.y = Mathf.Lerp(transform.localScale.y, Procentage * WantedHeight, Time.deltaTime * ChangeSpeed);

        transform.localScale = wantedScale;

        if (collider.enabled && Procentage < 0.1f)
            collider.enabled = false;
        else if (!collider.enabled && Procentage > 0.2f)
            collider.enabled = true;
    }

    void Update()
    {
        Health += ShieldRegeneration * Time.deltaTime;

        UpdateHeight();
    }

}
