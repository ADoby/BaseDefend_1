using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour {

	private string poolName = "";
	public Transform Owner;

	public Timer lifeTimer;
	public float ForceForward = 2.0f;
	[Range(0f, 1f)]
	public float Damping = 0.5f;

	public float ExplosionRange = 2.0f;
	public float ExplosionDamage = 10f;
	public float ExplosionForce = 100f;

	// Use this for initialization
	void Reset () 
	{
		rigidbody.velocity = Vector3.zero;
		lifeTimer.Reset();
	}

	void SetPoolName(string value)
	{
		poolName = value;
	}

	void Update()
	{
		if (lifeTimer.Update())
			Explode();
	}

	// Update is called once per frame
	void FixedUpdate() 
	{
		float delta = Time.fixedDeltaTime / Game.DefaultFixedTime;
		rigidbody.velocity += transform.forward * ForceForward * delta;
		rigidbody.velocity -= rigidbody.velocity * Damping * delta;
	}

	public LayerMask ExplosionMask;

	public void Explode()
	{
        if (ExplosionRange > 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRange, ExplosionMask);
            foreach (var item in hits)
            {
                TryDamage(item);
            }
        }
		GameObjectPool.Instance.Despawn(poolName, gameObject);
	}

	public void TryDamage(Collider item)
	{
        float procentage = 1f;
        if (ExplosionRange > 0)
            procentage = Vector3.Distance(item.transform.position, transform.position) / ExplosionRange;

		Rigidbody rig = item.rigidbody;
		if (rig)
		{
			rig.AddForce((item.transform.position - transform.position).normalized * procentage * ExplosionForce);
		}
		item.SendMessage("Damage", new vp_DamageInfo(procentage * ExplosionDamage, Owner), SendMessageOptions.DontRequireReceiver);
	}

	void OnTriggerEnter(Collider other)
	{
		TryDamage(other);
		Explode();
	}
}
