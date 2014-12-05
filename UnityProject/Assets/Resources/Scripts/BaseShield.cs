using UnityEngine;
using System.Collections;

public class BaseShield : HealthHandler
{
    public float ChangeSpeed = 2.0f;
    private float lastHeight = 1f;
    public float WantedHeight = 1f;
    private Vector3 wantedScale;

    public float HealthRegeneration = 1.0f;


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

        if (float.IsNaN(wantedScale.y))
            wantedScale.y = 0f;

        transform.localScale = wantedScale;

        if (collider.enabled && Procentage < 0.05f)
            collider.enabled = false;
        else if (!collider.enabled && Procentage > 0.1f)
            collider.enabled = true;
    }

    void Update()
    {
        Health += HealthRegeneration * Time.deltaTime;

        UpdateHeight();
    }

}
