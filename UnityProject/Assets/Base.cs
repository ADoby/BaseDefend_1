using UnityEngine;
using System.Collections;

public class Base : HealthHandler
{

	public override void Reset()
	{
		base.Reset();
	}

	// Use this for initialization
	void Start ()
	{
		Data.Instance.BaseHealthChanged.Send(this);
	}

	public override void Damage(vp_DamageInfo info)
	{
		base.Damage(info);
		Data.Instance.BaseHealthChanged.Send(this);
	}
}
