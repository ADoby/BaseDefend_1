using UnityEngine;
using System.Collections;

public class Sight : MonoBehaviour {

    public ZombieKI kiScript;
    public ZombieKI1 kiScript1;

    public Animator anim;

	// Use this for initialization
	void Start () {
        Reset();
	}

    public float fieldOfViewAngle = 180f;
    public float SightCheckTime = 0.2f;
    public LayerMask playermask;
    public float distance = 20.0f;

    Transform sightedPlayer = null;

    public bool needsTimeToRecognize = true;

    public float sightTime = 2.0f;
    public float sightTimer = 0.0f;

    public bool foundSomeone = false;

    private float delta = 0;
    private float lastDelta = 0;

    public float lookAtWeight = 0.0f;
    public float weightChange = 2.0f;

    public Transform head;

    public void Reset()
    {
        if(!anim)
            anim = GetComponent<Animator>();

        foundSomeone = false;
        sightedPlayer = null;
        lookAtWeight = 0.0f;


        lastDelta = Time.time;
        StartCoroutine(CheckSight());
    }

    void LateUpdate()
    {
        if (sightedPlayer && checkTargetInFrontOfPlayer(sightedPlayer))
        {
            anim.SetLookAtPosition(sightedPlayer.position);
            lookAtWeight = Mathf.Lerp(lookAtWeight, 1.0f, Time.deltaTime * weightChange);
        }
        else
        {
            lookAtWeight = Mathf.Lerp(lookAtWeight, 0f, Time.deltaTime * weightChange);
        }
        
        anim.SetLookAtWeight(lookAtWeight);
    }

    public IEnumerator CheckSight()
    {
        delta = Time.time - lastDelta;
        lastDelta = Time.time;

        foundSomeone = sightedPlayer != null ? true : false;

        if (needsTimeToRecognize && sightedPlayer)
        {
            if (checkSightToTarget(sightedPlayer))
            {
                if (kiScript)
                    kiScript.canSeeTarget = true;
                if (kiScript1)
                    kiScript1.canSeeTarget = true;

                sightTimer -= delta;
                if (sightTimer <= 0)
                {
                    if (kiScript)
                        kiScript.SetTargetPos(sightedPlayer.position);
                    if (kiScript1)
                        kiScript1.SetTargetPos(sightedPlayer.position);
                }

                yield return new WaitForSeconds(SightCheckTime);
                StartCoroutine(CheckSight());

                yield break;
            }
        }

        if (kiScript)
            kiScript.canSeeTarget = false;
        if (kiScript1)
            kiScript1.canSeeTarget = false;

        sightedPlayer = null;

        foreach (var item in Physics.OverlapSphere(head.position, distance, playermask))
        {
            if (item.gameObject.tag == "Player")
            {
                if (checkSightToTarget(item.transform))
                {
                    sightedPlayer = item.transform;
                    if (kiScript)
                        kiScript.canSeeTarget = true;
                    if (kiScript1)
                        kiScript1.canSeeTarget = true;
                    sightTimer = sightTime;
                    break;
                }
            }
        }

        if (!needsTimeToRecognize && sightedPlayer)
        {
            if (kiScript)
                kiScript.SetTargetPos(sightedPlayer.position);
            if (kiScript1)
                kiScript1.SetTargetPos(sightedPlayer.position);
        }
            

        yield return new WaitForSeconds(SightCheckTime);
        StartCoroutine(CheckSight());
    }

    private bool checkTargetInFrontOfPlayer(Transform target) 
    {
        Vector3 direction = target.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);
        if (angle < fieldOfViewAngle * 0.5f)
        {
            return true;
        }
        return false;
    }

    private bool checkTargetAngle(Transform target)
    {
        Vector3 direction = target.position - head.position;
        float angle = Vector3.Angle(direction, head.forward);
        if (angle < fieldOfViewAngle * 0.5f)
        {
            return true;
        }
        return false;
    }

    private bool checkSightToTarget(Transform target)
    {
        bool result = false;
        Vector3 direction = target.position - head.position;
        float angle = Vector3.Angle(direction, head.forward);
        if (angle < fieldOfViewAngle * 0.5f)
        {
            
            RaycastHit hit;
            if (Physics.Raycast(head.position, direction.normalized, out hit, distance, playermask))
                if (hit.transform == target)
                    result = true;
            if (Physics.Raycast(head.position + Vector3.up * 0.5f, direction.normalized, out hit, distance, playermask))
                if (hit.transform == target)
                    result = true;
            if (Physics.Raycast(head.position + Vector3.down * 0.5f, direction.normalized, out hit, distance, playermask))
                if (hit.transform == target)
                    result = true;
            if (Physics.Raycast(head.position + Vector3.right * 0.5f, direction.normalized, out hit, distance, playermask))
                if (hit.transform == target)
                    result = true;
            if (Physics.Raycast(head.position + Vector3.left * 0.5f, direction.normalized, out hit, distance, playermask))
                if (hit.transform == target)
                    result = true;
        }
        return result;
    }
}
