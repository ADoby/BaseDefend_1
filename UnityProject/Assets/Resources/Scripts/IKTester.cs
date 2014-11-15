using UnityEngine;
using System.Collections;

public class IKTester : MonoBehaviour {

    public Transform rightFoot, leftFoot;
    public Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();

        WantedRightFootPos = rightFoot.position;
        WantedLeftFootPos = leftFoot.position;
        RightFootPos = WantedRightFootPos;
        LeftFootPos = WantedLeftFootPos;

        agent = GetComponent<NavMeshAgent>();
        defaultBaseOffset = agent.baseOffset;
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    public float hitDegrees = 0.0f;

    public bool ikActive;

    public Transform RightHips;
    public Transform LeftHips;

    private float currentRightFoot, currentLeftFoot;

    public LayerMask footHit;

    private Vector3 WantedRightFootPos, WantedLeftFootPos;
    private Vector3 RightFootPos, LeftFootPos;
    private bool RightHit, LeftHit;

    public float PositionChangeSpeed = 2.0f;
    public float WeightChangeSpeed = 2.0f;

    public float heightTest = 1.2f;

    private NavMeshAgent agent;

    private float defaultBaseOffset = -0.05f;
    public float baseOffsetChangeSpeed = 2.0f;

    public float SpeedToWeight = 3.0f;

    void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                float maxDist = heightTest;

                //Nearly on the floor
                Ray ray = new Ray(RightHips.position, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, heightTest, footHit))
                {
                    hitDegrees = Mathf.Acos(Vector3.Dot(ray.direction, hit.normal));
                    WantedRightFootPos = hit.point + Vector3.up * 0.125f;
                    RightHit = true;
                    maxDist = hit.distance;
                }
                else
                {
                    RightHit = false;
                }

                ray = new Ray(LeftHips.position, Vector3.down);
                if (Physics.Raycast(ray, out hit, heightTest, footHit))
                {
                    hitDegrees = Mathf.Acos(Vector3.Dot(ray.direction, hit.normal));
                    WantedLeftFootPos = hit.point + Vector3.up * 0.125f;
                    LeftHit = true;
                    if (hit.distance > maxDist)
                    {
                        maxDist = hit.distance;
                    }
                }
                else
                {
                    LeftHit = false;
                }

                if (maxDist < heightTest)
                {
                    agent.baseOffset = Mathf.Lerp(agent.baseOffset, defaultBaseOffset - (maxDist - heightTest), Time.deltaTime * baseOffsetChangeSpeed);
                }else{
                    agent.baseOffset = Mathf.Lerp(agent.baseOffset, defaultBaseOffset, Time.deltaTime * baseOffsetChangeSpeed);
                }

                RightFootPos = Vector3.Lerp(RightFootPos, WantedRightFootPos, Time.deltaTime * PositionChangeSpeed);
                LeftFootPos = Vector3.Lerp(LeftFootPos, WantedLeftFootPos, Time.deltaTime * PositionChangeSpeed);

                anim.SetIKPosition(AvatarIKGoal.RightFoot, RightFootPos);
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootPos);

                currentRightFoot = 1.0f - Mathf.Clamp(Mathf.Abs(anim.GetFloat("Forward") * SpeedToWeight), 0.0f, 1.0f);
                currentLeftFoot = 1.0f - Mathf.Clamp(Mathf.Abs(anim.GetFloat("Forward") * SpeedToWeight), 0.0f, 1.0f);
               

                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, currentRightFoot);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, currentRightFoot);

                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, currentLeftFoot);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, currentLeftFoot);

            }
        }
    }
}
