using UnityEngine;
using System.Collections;

public class FallDownDeath : MonoBehaviour 
{

	
	// Update is called once per frame
	void Update () {
	    if(transform.position.y < -10)
        {
            SendMessage("Damage", 1000, SendMessageOptions.DontRequireReceiver);
        }
	}
}
