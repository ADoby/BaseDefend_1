using UnityEngine;
using System.Collections;

public class Base : HealthHandler
{
    #region Singleton
    private static Base instance;
    public static Base Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<Base>();
            return instance;
        }
    }
    protected void Awake()
    {
        instance = this;
        Reset();
    }
    #endregion

    public Transform NavigationTarget;

	public override void Reset()
	{
		base.Reset();
	}

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

	// Use this for initialization
	void Start ()
	{
		Data.Instance.BaseHealthChanged.Send(this);
	}

    void Update()
    {
        Health += HealthRegeneration * Time.deltaTime;
    }

	public override void Damage(vp_DamageInfo info)
	{
		base.Damage(info);
		Data.Instance.BaseHealthChanged.Send(this);
	}

    public override void Despawn()
    {
        Game.TriggerGameOver();
    }

    public float HealthRegeneration = 0f;
}
