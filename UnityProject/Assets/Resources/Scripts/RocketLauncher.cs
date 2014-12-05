using UnityEngine;
using System.Collections;

public class RocketLauncher : MonoBehaviour 
{
	public Transform Owner;

	public float RotateSpeed = 1.0f;
	private string rocketPool = "Robot1_Rocket";
	public Transform Rocket_Pos1;
	public Transform target;

	public float MaxAngle = 20;
	public float angle = 0;


	// Update is called once per frame
	void Update()
	{
		float delta = Game.EnemyDelta;

		Vector3 diff = Rocket_Pos1.localPosition;
		Vector3 currentDirection = transform.forward;
		Vector3 lookDirection = Owner.forward;
		if (target)
		{
            lookDirection = target.position - (transform.position - diff);
			angle = Vector3.Angle(Owner.forward, lookDirection);
			if (angle > MaxAngle)
				lookDirection = Owner.forward;
		}

		

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDirection), Mathf.Min(delta * RotateSpeed, 1f));
	}
    
	public bool Shoot(float damage)
	{
		if (angle > MaxAngle)
			return false;

        Vector3 direction = target.position - Rocket_Pos1.position;

		GameObject go = GameObjectPool.Instance.Spawn(rocketPool, Rocket_Pos1.position, Quaternion.LookRotation(direction));
        if (go == null)
            return false;
		Rocket rocket = go.GetComponent<Rocket>();
		if (rocket)
		{
			rocket.Owner = this.Owner;
            rocket.ExplosionDamage = damage;
		}
		return true;
	}
}
