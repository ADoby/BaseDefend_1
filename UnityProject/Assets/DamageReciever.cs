using UnityEngine;
using System.Collections;

public class DamageReciever : MonoBehaviour 
{

    public HealthHandler reciever;
    public vp_DamageHandler reciever2;

    public float DamageMultiply = 1.0f;

    public void Damage(vp_DamageInfo damageInfo)
    {
        if (reciever)
        {
            damageInfo.Damage *= DamageMultiply;
            reciever.Damage(damageInfo);
        }
        if (reciever2)
        {
            damageInfo.Damage *= DamageMultiply;
            reciever2.Damage(damageInfo);
        }
    }

    public void Damage(float damage)
    {
        if (reciever)
        {
            damage *= DamageMultiply;
            reciever.Damage(damage);
        }
        if (reciever2)
        {
            damage *= DamageMultiply;
            reciever2.Damage(damage);
        }
    }
}
