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

    public BaseShield Shield1, Shield2;

    public Transform AutoTurretPos;
    public bool AutoTurret = true;
    public float AutoTurretRange = 15.0f;
    public Timer AutoTurretShootTimer;
    public LayerMask AutoTurretMask;
    public LayerMask AutoTurretSightMask;

    public int RocketDamage = 10;
    public float RocketSpeed = 2.0f;
    public float RocketTargetingSpeed = 2.0f;
    public float RocketCooldown
    {
        get
        {
            return AutoTurretShootTimer.Value;
        }
        set
        {
            AutoTurretShootTimer.Value = value;
        }
    }

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
        Data.Instance.BaseHealthChanged.Send(this);

        FindTarget();
        if (AutoTurretShootTimer.Update())
        {
            if (target != null)
            {
                AutoTurretShootTimer.Reset();
                Shoot();
            }
        }
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

    public string rocketPool;

    public void Shoot()
    {
        Vector3 direction = (target.GetBody.position + Vector3.up) - AutoTurretPos.position;

        GameObject go = GameObjectPool.Instance.Spawn(rocketPool, AutoTurretPos.position, Quaternion.LookRotation(direction));
        if (go == null)
            return;
        Rocket rocket = go.GetComponent<Rocket>();
        if (rocket)
        {
            rocket.Owner = AutoTurretPos;
            rocket.ExplosionDamage = RocketDamage;
            rocket.ForceForward = RocketSpeed;
            rocket.RotateSpeed = RocketTargetingSpeed;
            rocket.target = target.GetBody;
        }
    }

    public HealthHandler target;

    public void FindTarget()
    {
        if (target)
        {
            RaycastHit hit;
            if (Physics.Raycast(AutoTurretPos.position, ((target.GetBody.position + Vector3.up) - AutoTurretPos.position).normalized, out hit, AutoTurretRange, AutoTurretSightMask))
            {
                if (hit.transform != target)
                    target = null;
            }
            else
            {
                target = null;
            }
            if (target && target.isDead)
                target = null;
        }
        if (target)
            return;
        Collider[] colliders = Physics.OverlapSphere(AutoTurretPos.position, AutoTurretRange, AutoTurretMask);
        foreach (var item in colliders)
        {
            RaycastHit hit;
            if (Physics.Raycast(AutoTurretPos.position, ((item.transform.position + Vector3.up) - AutoTurretPos.position).normalized, out hit, AutoTurretRange, AutoTurretSightMask))
            {
                if (hit.collider == item)
                {
                    HealthHandler handler = item.GetComponent<HealthHandler>();
                    if (!handler)
                    {
                        ColliderHit hitCollider = item.GetComponent<ColliderHit>();
                        if(hitCollider)
                        {
                            handler = hitCollider.healthScript;
                        }
                    }
                    if (!handler)
                        continue;
                    if (handler.isDead)
                        continue;
                    target = handler;
                    return;
                }
            }
        }
    }

    public float HealthRegeneration = 0f;
}
