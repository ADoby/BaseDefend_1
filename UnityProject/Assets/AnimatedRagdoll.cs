using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class AnimatedRagdoll : MonoBehaviour {

	public MeshRenderer AnimatedMesh;
	public MeshRenderer RagdolledMesh;
	public Animator AnimatedAnimator;
	public Animator RagdolledAnimator;

	[SerializeField]
	List<RagdollBodyPart> bodyParts = new List<RagdollBodyPart>();

	public bool ragdolled = false;

	// Use this for initialization
	void Start () 
	{
		bodyParts = new List<RagdollBodyPart>();
		HumanBodyBones[] bones = (HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones));
		for (int i = 0; i < bones.Length; i++)
		{
            Transform animatedTransform = AnimatedAnimator.GetBoneTransform(bones[i]);
            Transform ragdolledTransform = RagdolledAnimator.GetBoneTransform(bones[i]);

			if (animatedTransform && ragdolledTransform)
			{
				if (ragdolledTransform.rigidbody)
				{
					RagdollBodyPartCollider part = ragdolledTransform.gameObject.AddComponent<RagdollBodyPartCollider>();
					part.script = this;
					part.part = bones[i];
					part.Init(AnimatedAnimator, RagdolledAnimator);

					bodyParts.Add(part);
				}
				else
				{
					RagdollBodyPart part = ragdolledTransform.gameObject.AddComponent<RagdollBodyPart>();
					part.script = this;
					part.part = bones[i];
					part.Init(AnimatedAnimator, RagdolledAnimator);

					bodyParts.Add(part);
				}
				
			}
		}
	}

	public void Ragdoll(bool value)
	{
		ragdolled = value;
		if (ragdolled)
		{
			for (int i = 0; i < bodyParts.Count; i++)
			{
				bodyParts[i].strength = 0f;
			}
		}
	}

    public Rigidbody hip;

    void FixedUpdate()
    {
        if (helper.ragdolled && hip.velocity.magnitude < 0.1f)
        {
            helper.ragdolled = false;
        }
        else if(ragdolled)
        {
            if (helper.state == RagdollHelper.RagdollState.animated)
            {
                Ragdoll(false);
            }
        }
    }

    public RagdollHelper helper;

	public void Hit(RagdollBodyPart part)
	{
		switch (part.part)
		{
			case HumanBodyBones.Chest:
				break;
			case HumanBodyBones.Head:
                helper.ragdolled = true;
                Ragdoll(true);
				break;
			case HumanBodyBones.Hips:
                helper.ragdolled = true;
                Ragdoll(true);
				break;
			case HumanBodyBones.Jaw:
				break;
			case HumanBodyBones.LastBone:
				break;
			case HumanBodyBones.LeftEye:
				break;
			case HumanBodyBones.LeftFoot:
				break;
			case HumanBodyBones.LeftHand:
				break;
			case HumanBodyBones.LeftIndexDistal:
				break;
			case HumanBodyBones.LeftIndexIntermediate:
				break;
			case HumanBodyBones.LeftIndexProximal:
				break;
			case HumanBodyBones.LeftLittleDistal:
				break;
			case HumanBodyBones.LeftLittleIntermediate:
				break;
			case HumanBodyBones.LeftLittleProximal:
				break;
			case HumanBodyBones.LeftLowerArm:
				break;
			case HumanBodyBones.LeftLowerLeg:
				break;
			case HumanBodyBones.LeftMiddleDistal:
				break;
			case HumanBodyBones.LeftMiddleIntermediate:
				break;
			case HumanBodyBones.LeftMiddleProximal:
				break;
			case HumanBodyBones.LeftRingDistal:
				break;
			case HumanBodyBones.LeftRingIntermediate:
				break;
			case HumanBodyBones.LeftRingProximal:
				break;
			case HumanBodyBones.LeftShoulder:
				break;
			case HumanBodyBones.LeftThumbDistal:
				break;
			case HumanBodyBones.LeftThumbIntermediate:
				break;
			case HumanBodyBones.LeftThumbProximal:
				break;
			case HumanBodyBones.LeftToes:
				break;
			case HumanBodyBones.LeftUpperArm:

				break;
			case HumanBodyBones.LeftUpperLeg:
				break;
			case HumanBodyBones.Neck:
				break;
			case HumanBodyBones.RightEye:
				break;
			case HumanBodyBones.RightFoot:
				break;
			case HumanBodyBones.RightHand:
				break;
			case HumanBodyBones.RightIndexDistal:
				break;
			case HumanBodyBones.RightIndexIntermediate:
				break;
			case HumanBodyBones.RightIndexProximal:
				break;
			case HumanBodyBones.RightLittleDistal:
				break;
			case HumanBodyBones.RightLittleIntermediate:
				break;
			case HumanBodyBones.RightLittleProximal:
				break;
			case HumanBodyBones.RightLowerArm:
				break;
			case HumanBodyBones.RightLowerLeg:
				break;
			case HumanBodyBones.RightMiddleDistal:
				break;
			case HumanBodyBones.RightMiddleIntermediate:
				break;
			case HumanBodyBones.RightMiddleProximal:
				break;
			case HumanBodyBones.RightRingDistal:
				break;
			case HumanBodyBones.RightRingIntermediate:
				break;
			case HumanBodyBones.RightRingProximal:
				break;
			case HumanBodyBones.RightShoulder:
				break;
			case HumanBodyBones.RightThumbDistal:
				break;
			case HumanBodyBones.RightThumbIntermediate:
				break;
			case HumanBodyBones.RightThumbProximal:
				break;
			case HumanBodyBones.RightToes:
				break;
			case HumanBodyBones.RightUpperArm:
				break;
			case HumanBodyBones.RightUpperLeg:
				break;
			case HumanBodyBones.Spine:
				break;
			default:
				break;
		}
	}
}
