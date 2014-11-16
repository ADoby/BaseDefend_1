using UnityEngine;
using System.Collections;

[System.Serializable]
public class RagdollBodyPart : MonoBehaviour {

    public HumanBodyBones part;

    public AnimatedRagdoll script;

    public void Init(Animator animatedAnimator, Animator ragdolledAnimator)
    {
        animatedTransform = animatedAnimator.GetBoneTransform(part);
        ragdolledTransform = ragdolledAnimator.GetBoneTransform(part);

        ragdolledRigidbody = ragdolledTransform.rigidbody;

        if (ragdolledRigidbody)
        {
            UseRigidbody = true;

            ragdolledRigidbody.useGravity = true;
            ragdolledRigidbody.drag = 0f;
            ragdolledRigidbody.angularDrag = 0f;
        }
    }

    public void LoseStrengthAll()
    {
        float loseStrength = strengthFunction;

        LoseStrength(loseStrength);

        loseStrength *= strengthLoseFalloff;
        LoseStrengthChildren(loseStrength);

        //LoseStrengthParents(loseStrength, transform);
    }

    public void LoseStrength(float loseStrength)
    {
        strengthFunction = 0f;
        //strengthFunction = Mathf.Clamp(strengthFunction - loseStrength, 0f, 2000f);
    }

    private float strengthLoseFalloff = 1f;

    public void LoseStrengthParents(float loseStrength, Transform currentTransform)
    {
        RagdollBodyPart parentScript = currentTransform.parent.GetComponent<RagdollBodyPart>();
        if (!parentScript)
            return;

        parentScript.LoseStrength(loseStrength);

        loseStrength *= strengthLoseFalloff;

        parentScript.LoseStrengthChildren(loseStrength, currentTransform);

        LoseStrengthParents(loseStrength, currentTransform.parent);
    }

    public void LoseStrengthChildren(float loseStrength, Transform ignoreChild = null)
    {
        loseStrength *= strengthLoseFalloff;
        foreach (Transform item in transform)
        {
            if (ignoreChild != item)
            {
                RagdollBodyPart child = item.GetComponent<RagdollBodyPart>();
                if (child)
                {
                    child.LoseStrength(loseStrength);
                    child.LoseStrengthChildren(loseStrength);
                }
            }
        }
    }

    public float strength = 0f;
    private float strengthFunction = 0f;

    private float strengthChange = 2f;

    void Update()
    {
        if (!script.ragdolled)
        {
            strengthFunction += Time.deltaTime * strengthChange;

            strength = Mathf.Clamp(strengthFunction - 0.5f, 0f, 1f);
        }
        
    }

    void FixedUpdate()
    {
        if (!script.ragdolled)
        {
            UpdatePosition();
        }
    }

    public virtual void UpdatePosition()
    {
        RagdollTrans.localPosition = Vector3.Lerp(RagdollTrans.localPosition, AnimTrans.localPosition, strength);
        RagdollTrans.localRotation = Quaternion.Slerp(RagdollTrans.localRotation, AnimTrans.localRotation, strength);
    }

    [SerializeField]
    protected bool enabled = true;

    public bool UseRigidbody = false;

    [SerializeField]
    protected Transform animatedTransform;
    [SerializeField]
    protected Transform ragdolledTransform;
    [SerializeField]
    protected Rigidbody ragdolledRigidbody;

    public Transform AnimTrans
    {
        get
        {
            return animatedTransform;
        }
    }

    public Transform RagdollTrans
    {
        get
        {
            return ragdolledTransform;
        }
    }

    public Rigidbody RagdollRigid
    {
        get
        {
            return ragdolledRigidbody;
        }
    }

    public bool Enabled
    {
        get
        {
            return enabled;
        }
    }
}
