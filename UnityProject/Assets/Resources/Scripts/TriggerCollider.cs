using UnityEngine;
using System.Collections;

public class TriggerCollider : MonoBehaviour {

    public float force = 2.0f;

    public float maxDistance = 1.5f;

    public Transform forcePos;

    void OnTriggerStay(Collider other)
    {
        Vector3 distance1 = forcePos.position - other.transform.position;
        Vector3 distance2 = other.transform.position - forcePos.position;

        distance1.y = distance2.y = forcePos.position.y;

        Debug.DrawRay(forcePos.position, distance2);

        Vector3 direction = Mathf.Abs(maxDistance - distance2.magnitude) * distance1.normalized;

        Debug.DrawRay(forcePos.position, direction, Color.red);

        rigidbody.AddForceAtPosition(direction * force, forcePos.position);
    }
}
