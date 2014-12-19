using UnityEngine;
using System.Collections;

public class PlayerHealthRegeneration : MonoBehaviour 
{

    public float ValuePerSecond = 0f;
    public vp_FPPlayerDamageHandler damageHandler;
	
	// Update is called once per frame
	void Update ()
    {
        if (ValuePerSecond > 0)
        {
            damageHandler.CurrentHealth = Mathf.Min(damageHandler.CurrentHealth + ValuePerSecond * Time.deltaTime, damageHandler.MaxHealth);
        }
	}
}
