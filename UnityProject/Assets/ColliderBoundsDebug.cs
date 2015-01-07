using UnityEngine;
using System.Collections;

public class ColliderBoundsDebug : MonoBehaviour {

    public BoxCollider Collider;
	
	// Update is called once per frame
	void Update () 
    {
        Vector3 min = Collider.bounds.min;
        Vector3 max = Collider.bounds.max;

        Vector3 pos1 = min;
        Vector3 pos2 = min;
        pos2.x = max.x;
        Vector3 pos3 = min;
        pos3.x = max.x;
        pos3.z = max.z;
        Vector3 pos4 = min;
        pos4.z = max.z;

        Vector3 pos5 = pos1;
        pos5.y = max.y;
        Vector3 pos6 = pos2;
        pos6.y = max.y;
        Vector3 pos7 = pos3;
        pos7.y = max.y;
        Vector3 pos8 = pos4;
        pos8.y = max.y;

        Debug.DrawLine(pos1, pos2);
        Debug.DrawLine(pos2, pos3);
        Debug.DrawLine(pos3, pos4);
        Debug.DrawLine(pos4, pos1);

        Debug.DrawLine(pos5, pos6);
        Debug.DrawLine(pos6, pos7);
        Debug.DrawLine(pos7, pos8);
        Debug.DrawLine(pos8, pos5);
	}
}
