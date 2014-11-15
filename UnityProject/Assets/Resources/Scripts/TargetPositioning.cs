using UnityEngine;
using System.Collections;

public class TargetPositioning : MonoBehaviour {

    public float maxDistance = 100.0f;
    public Transform target;

    public float updateTime = 0.1f;

	// Update is called once per frame
	void Start () {
        StartCoroutine(UpdatePosition());
	}

    IEnumerator UpdatePosition()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            target.position = hit.point;
        }
        else
        {
            target.position = ray.origin + ray.direction * maxDistance;
        }

        yield return new WaitForSeconds(updateTime);
        StartCoroutine(UpdatePosition());
    }
}
