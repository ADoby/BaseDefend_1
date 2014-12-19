using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageOnTriggerStay : MonoBehaviour
{

    public float DamagePerSecond = 5;

    public void OnTriggerStay(Collider other)
    {
        vp_DamageHandler handler = other.GetComponent<vp_DamageHandler>();
        if (handler)
            handler.Damage(new vp_DamageInfo(DamagePerSecond * Time.deltaTime, transform));
    }
}
