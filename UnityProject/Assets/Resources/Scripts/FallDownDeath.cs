using UnityEngine;
using System.Collections;

public class FallDownDeath : MonoBehaviour 
{

	
	// Update is called once per frame
	void Update () {
	    if(transform.position.y < -10)
        {
            vp_DamageInfo info = new vp_DamageInfo(1000f, null);

            SendMessage("Damage", info, SendMessageOptions.DontRequireReceiver);
        }
	}
}
